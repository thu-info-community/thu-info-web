using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using ThuInfoWeb.Dtos;
using Version = ThuInfoWeb.DBModels.Version;

namespace ThuInfoWeb;

public sealed class VersionManager(
    ILogger<VersionManager> logger,
    Data data,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    IHttpClientFactory httpClientFactory) : IHostedService
{
    private const string ApkPublicBaseUrl = "https://app.cs.tsinghua.edu.cn/apk/";
    private const string InternalAndroidVersionUrl = "https://stu.cs.tsinghua.edu.cn/thuinfo/version/android";
    private const string InternalIosVersionUrl = "https://stu.cs.tsinghua.edu.cn/thuinfo/version/ios";
    private const string GithubLatestReleaseUrl =
        "https://api.github.com/repos/thu-info-community/thu-info-app/releases/latest";
    private const string AppStoreLookupUrl = "https://itunes.apple.com/lookup?id=1533968428";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public enum OS
    {
        Android,
        IOS
    }

    private readonly ILogger<VersionManager> _logger = logger;
    private readonly Data _data = data;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly HttpClient _client = httpClientFactory.CreateClient("version-manager");
    private readonly bool _internalNetworkMode = configuration.GetValue("InternalNetworkMode", false);
    private readonly Lock _stateLock = new();
    private Version _currentVersionOfAndroid = new();
    private Version _currentVersionOfIos = new();
    private bool _isRunning;

    public bool IsRunning
    {
        get
        {
            lock (_stateLock)
                return _isRunning;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var versions = await Task.WhenAll(
                _data.GetVersionAsync(true),
                _data.GetVersionAsync(false));

            cancellationToken.ThrowIfCancellationRequested();

            lock (_stateLock)
            {
                _currentVersionOfAndroid = versions[0] ?? new Version();
                _currentVersionOfIos = versions[1] ?? new Version();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            ApplicationLog.VersionCacheInitializationFailed(_logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public VersionDto GetCurrentVersion(OS os)
    {
        Version version;
        lock (_stateLock)
            version = os == OS.Android ? _currentVersionOfAndroid : _currentVersionOfIos;

        return os switch
        {
            OS.Android => new VersionDto
            {
                CreatedTime = version.CreatedTime,
                DownloadUrl = GetAndroidApkUrl(version.VersionName),
                ReleaseNote = version.ReleaseNote,
                VersionName = version.VersionName
            },
            OS.IOS => new VersionDto
            {
                CreatedTime = version.CreatedTime,
                DownloadUrl = "https://apps.apple.com/cn/app/thu-info/id1533968428",
                ReleaseNote = version.ReleaseNote,
                VersionName = version.VersionName
            },
            _ => throw new ArgumentOutOfRangeException(nameof(os), os, null)
        };
    }

    public async Task CheckUpdateAsync(OS os, CancellationToken cancellationToken = default)
    {
        lock (_stateLock)
        {
            if (_isRunning)
                return;
            _isRunning = true;
        }

        var osName = os == OS.Android ? "Android" : "iOS";
        try
        {
            var current = GetCurrentVersion(os);
            ApplicationLog.VersionCheckStarted(_logger, osName, current.VersionName);

            var version = _internalNetworkMode
                ? await GetInternalVersionAsync(os, cancellationToken)
                : await GetPublicVersionAsync(os, cancellationToken);

            if (version is null || !IsNewerVersion(version.VersionName, current.VersionName))
            {
                ApplicationLog.NoNewerVersion(_logger, osName);
                return;
            }

            if (os == OS.Android && !AndroidApkExists(version.VersionName))
                return;

            if (await _data.CreateVersionAsync(version) != 1)
                throw new InvalidOperationException("The new application version could not be saved.");

            ApplicationLog.VersionFound(_logger, osName, version.VersionName);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ApplicationLog.VersionCheckCanceled(_logger, osName);
        }
        catch (Exception ex)
        {
            ApplicationLog.VersionCheckFailed(_logger, ex, osName);
        }
        finally
        {
            try
            {
                var version = await _data.GetVersionAsync(os == OS.Android);
                if (version is not null)
                {
                    lock (_stateLock)
                    {
                        if (os == OS.Android)
                            _currentVersionOfAndroid = version;
                        else
                            _currentVersionOfIos = version;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLog.VersionCacheRefreshFailed(_logger, ex, osName);
            }

            lock (_stateLock)
                _isRunning = false;
        }
    }

    private async Task<Version?> GetInternalVersionAsync(OS os, CancellationToken cancellationToken)
    {
        var url = os == OS.Android ? InternalAndroidVersionUrl : InternalIosVersionUrl;
        return await _client.GetFromJsonAsync<Version>(url, JsonOptions, cancellationToken);
    }

    private async Task<Version?> GetPublicVersionAsync(OS os, CancellationToken cancellationToken)
    {
        if (os == OS.Android)
        {
            var release = await _client.GetFromJsonAsync<GithubRelease>(GithubLatestReleaseUrl, JsonOptions,
                cancellationToken);
            if (release?.TagName is null || release.PublishedAt is null)
                return null;

            return new Version
            {
                CreatedTime = release.PublishedAt.Value.LocalDateTime,
                IsAndroid = true,
                ReleaseNote = release.Body ?? string.Empty,
                VersionName = release.TagName
            };
        }

        var lookup = await _client.GetFromJsonAsync<AppStoreLookup>(AppStoreLookupUrl, JsonOptions,
            cancellationToken);
        var result = lookup?.Results?.FirstOrDefault();
        if (result?.Version is null || result.CurrentVersionReleaseDate is null)
            return null;

        return new Version
        {
            CreatedTime = result.CurrentVersionReleaseDate.Value.LocalDateTime,
            IsAndroid = false,
            ReleaseNote = result.ReleaseNotes ?? string.Empty,
            VersionName = result.Version
        };
    }

    private bool AndroidApkExists(string versionName)
    {
        var apkPath = Path.Combine(_environment.WebRootPath, "apk", GetAndroidApkFileName(versionName));
        if (File.Exists(apkPath))
            return true;

        ApplicationLog.AndroidApkMissing(_logger, versionName, apkPath);
        return false;
    }

    private static bool IsNewerVersion(string? candidate, string? current)
    {
        var candidateNumber = NormalizeVersion(candidate);
        if (!candidateNumber.IsValidVersionNumber())
            return false;

        var currentNumber = NormalizeVersion(current);
        return string.IsNullOrEmpty(currentNumber)
               || !currentNumber.IsValidVersionNumber()
               || candidateNumber.VersionGreaterThan(currentNumber);
    }

    private static string NormalizeVersion(string? version)
    {
        var trimmed = version?.Trim() ?? string.Empty;
        return trimmed.Length > 0 && (trimmed[0] == 'v' || trimmed[0] == 'V') ? trimmed[1..] : trimmed;
    }

    private static string GetAndroidApkUrl(string versionName)
    {
        var versionNumber = NormalizeVersion(versionName);
        return !versionNumber.IsValidVersionNumber()
            ? string.Empty
            : ApkPublicBaseUrl + Uri.EscapeDataString($"THUInfo_release_v{versionNumber}.apk");
    }

    private static string GetAndroidApkFileName(string versionName)
    {
        var versionNumber = NormalizeVersion(versionName);
        if (!versionNumber.IsValidVersionNumber())
            throw new ArgumentException("Invalid Android version number format.", nameof(versionName));

        return $"THUInfo_release_v{versionNumber}.apk";
    }

    private sealed record GithubRelease(
        [property: JsonPropertyName("tag_name")] string? TagName,
        [property: JsonPropertyName("published_at")] DateTimeOffset? PublishedAt,
        [property: JsonPropertyName("body")] string? Body);

    private sealed record AppStoreLookup(
        [property: JsonPropertyName("results")] AppStoreResult[]? Results);

    private sealed record AppStoreResult(
        [property: JsonPropertyName("version")] string? Version,
        [property: JsonPropertyName("currentVersionReleaseDate")] DateTimeOffset? CurrentVersionReleaseDate,
        [property: JsonPropertyName("releaseNotes")] string? ReleaseNotes);
}
