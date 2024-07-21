using System.Text.Json;
using System.Text.Json.Nodes;
using ThuInfoWeb.Dtos;
using Version = ThuInfoWeb.DBModels.Version;

namespace ThuInfoWeb;

public class VersionManager(ILogger<VersionManager> logger, Data data, IConfiguration configuration)
{
    public enum OS
    {
        Android,
        IOS
    }

    private readonly HttpClient _client = new Func<HttpClient>(() =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("user-agent", "aspnetcore/6.0");
        return client;
    })();

    private readonly bool _internalNetworkMode = bool.Parse(configuration["InternalNetworkMode"] ?? "false");
    private readonly object _lock = new();
    private Version _currentVersionOfAndroid = data.GetVersionAsync(true).Result ?? new Version();
    private Version _currentVersionOfIOS = data.GetVersionAsync(false).Result ?? new Version();
    private bool _isRunning;

    public bool IsRunning
    {
        get
        {
            lock (_lock)
            {
                return _isRunning;
            }
        }
        private set
        {
            lock (_lock)
            {
                _isRunning = value;
            }
        }
    }

    public VersionDto GetCurrentVersion(OS os)
    {
        return os switch
        {
            OS.Android => new VersionDto
            {
                CreatedTime = _currentVersionOfAndroid.CreatedTime,
                DownloadUrl = "https://app.cs.tsinghua.edu.cn/api/apk",
                ReleaseNote = _currentVersionOfAndroid.ReleaseNote,
                VersionName = _currentVersionOfAndroid.VersionName
            },
            OS.IOS => new VersionDto
            {
                CreatedTime = _currentVersionOfIOS.CreatedTime,
                DownloadUrl = "https://apps.apple.com/cn/app/thu-info/id1533968428",
                ReleaseNote = _currentVersionOfIOS.ReleaseNote,
                VersionName = _currentVersionOfIOS.VersionName
            },
            _ => throw new ArgumentOutOfRangeException(nameof(os), os, null)
        };
    }

    public async Task CheckUpdateAsync(OS os)
    {
        IsRunning = true;
        logger.LogInformation("Start checking update for {OS}, current version is {Version}",
            os == OS.Android ? "Android" : "iOS",
            os == OS.Android ? _currentVersionOfAndroid.VersionName : _currentVersionOfIOS.VersionName);

        try
        {
            if (_internalNetworkMode)
            {
                if (os == OS.Android)
                {
                    var content = await _client.GetStringAsync(
                        "https://stu.cs.tsinghua.edu.cn/thuinfo/version/android");
                    var version = JsonSerializer.Deserialize<Version>(content)!;
                    if (version.VersionName == _currentVersionOfAndroid.VersionName)
                        logger.LogInformation("No newer version is available for Android (current version is {VersionName}),"
                                              + " check update for Android ok", version.VersionName);
                    
                    if (await data.CreateVersionAsync(version) != 1)
                        throw new Exception("Unknown Error");
                    logger.LogInformation("Found new version for Android: {VersionName}, check update ok",
                        version.VersionName);
                }
                else
                {
                    var content = await _client.GetStringAsync("https://stu.cs.tsinghua.edu.cn/thuinfo/version/ios");
                    var version = JsonSerializer.Deserialize<Version>(content)!;
                    if (version.VersionName == _currentVersionOfIOS.VersionName)
                        logger.LogInformation("No newer version is available for iOS(current version is {VersionName}), check update for iOS ok", version.VersionName);

                    if (await data.CreateVersionAsync(version) != 1)
                        throw new Exception("Unknown Error");
                    logger.LogInformation("Found new version for iOS: {VersionName}, check update for iOS ok", version.VersionName);
                }
            }
            else
            {
                if (os == OS.Android)
                {
                    const string url = "https://api.github.com/repos/UNIDY2002/THUInfo/releases/latest";
                    var content = await _client.GetStringAsync(url);
                    var json = JsonNode.Parse(content)!;
                    var versionName = (string)json["name"]!;
                    if (versionName == _currentVersionOfAndroid.VersionName)
                    {
                        logger.LogInformation(
                            "No newer version is available for Android(current version is {VersionName}), check update for Android ok",
                            versionName);
                    }
                    else
                    {
                        var publishedAt = DateTime.Parse((string)json["published_at"]!).ToLocalTime();
                        var releaseNote = (string)json["body"]!;
                        var version = new Version
                        {
                            CreatedTime = publishedAt,
                            IsAndroid = true,
                            ReleaseNote = releaseNote,
                            VersionName = versionName
                        };
                        var result = await data.CreateVersionAsync(version);
                        if (result != 1)
                            throw new Exception("Unknown Error");
                        logger.LogInformation("Found new version for Android: {VersionName}, check update ok", versionName);
                    }
                }
                else // handle ios
                {
                    const string url = "https://itunes.apple.com/lookup?id=1533968428";
                    var content = await _client.GetStringAsync(url);
                    var json = JsonNode.Parse(content)!["results"]!.AsArray()[0]!;
                    var versionName = (string)json["version"]!;
                    if (versionName == _currentVersionOfIOS.VersionName)
                    {
                        logger.LogInformation(
                            "No newer version is available for iOS(current version is {VersionName}), check update for iOS ok",
                            versionName);
                    }
                    else
                    {
                        var publishedAt = DateTime.Parse((string)json["currentVersionReleaseDate"]!).ToLocalTime();
                        var releaseNote = (string)json["releaseNotes"]!;
                        var version = new Version
                        {
                            CreatedTime = publishedAt,
                            IsAndroid = false,
                            ReleaseNote = releaseNote,
                            VersionName = versionName
                        };
                        var result = await data.CreateVersionAsync(version);
                        if (result != 1)
                            throw new Exception("Unknown Error");
                        logger.LogInformation("Found new version for iOS: {VersionName}, check update for iOS ok", versionName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Checking update for {OS} failed", os);
        }
        finally
        {
            var version = await data.GetVersionAsync(os == OS.Android) ?? new Version();
            if (os == OS.Android)
                _currentVersionOfAndroid = version;
            else
                _currentVersionOfIOS = version;
            IsRunning = false;
        }
    }
}
