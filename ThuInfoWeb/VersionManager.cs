using System.Text.Json;
using System.Text.Json.Nodes;
using ThuInfoWeb.Dtos;
using Version = ThuInfoWeb.DBModels.Version;

namespace ThuInfoWeb
{
    public class VersionManager
    {
        private readonly ILogger<VersionManager> _logger;
        private readonly Data _data;
        private readonly HttpClient _client = new Func<HttpClient>(() =>
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "aspnetcore/6.0");
            return client;
        })();
        private Version _currentVersionOfAndroid;
        private Version _currentVersionOfIOS;
        private readonly object _lock = new();
        private bool isRunning;
        public bool IsRunning
        {
            get
            {
                lock (_lock)
                {
                    return isRunning;
                }
            }
            private set
            {
                lock (_lock)
                {
                    isRunning = value;
                }
            }
        }

        private readonly bool _internalNetworkMode;

        public VersionManager(ILogger<VersionManager> logger, Data data,IConfiguration configuration)
        {
            this._logger = logger;
            this._data = data;
            this._internalNetworkMode = bool.Parse(configuration["InternalNetworkMode"]);
            // initial current version from database in ctor.
            this._currentVersionOfAndroid = data.GetVersionAsync(true).Result ?? new Version();
            this._currentVersionOfIOS = data.GetVersionAsync(false).Result ?? new Version();
        }
        public VersionDto GetCurrentVersion(OS os) => os switch
        {
            OS.Android => new VersionDto
            {
                CreatedTime = _currentVersionOfAndroid.CreatedTime,
                DownloadUrl = "https://thuinfo.net/api/apk",
                ReleaseNote = _currentVersionOfAndroid.ReleaseNote,
                VersionName = _currentVersionOfAndroid.VersionName,
            },
            OS.IOS => new VersionDto
            {
                CreatedTime = _currentVersionOfIOS.CreatedTime,
                DownloadUrl = "https://apps.apple.com/cn/app/thu-info/id1533968428",
                ReleaseNote = _currentVersionOfIOS.ReleaseNote,
                VersionName = _currentVersionOfIOS.VersionName
            }
        };
        public async Task CheckUpdateAsync(OS os)
        {
            IsRunning = true;
            _logger.LogInformation($"Start checking update for {(os == OS.Android ? "Android" : "iOS")}, current version is {(os == OS.Android ? _currentVersionOfAndroid.VersionName : _currentVersionOfIOS.VersionName)}");
            try
            {
                if (_internalNetworkMode)
                {
                    if (os == OS.Android)
                    {
                        var content = await _client.GetStringAsync(
                            "https://stu.cs.tsinghua.edu.cn/thuinfo/version/android");
                        var version = JsonSerializer.Deserialize<Version>(content)!;
                        if(version.VersionName == _currentVersionOfAndroid.VersionName)
                            _logger.LogInformation(
                                $"No newer version is available for Android(current version is {version.VersionName}), check update for Android ok.");
                        var result = await _data.CreateVersionAsync(version);
                        if (result != 1) throw new Exception("Unknown Error");
                        else
                            _logger.LogInformation(
                                $"Found new version for Android: {version.VersionName}, check update ok");
                    }
                    else
                    {
                        var content = await _client.GetStringAsync(
                            "https://stu.cs.tsinghua.edu.cn/thuinfo/version/ios");
                        var version = JsonSerializer.Deserialize<Version>(content)!;
                        if(version.VersionName == _currentVersionOfIOS.VersionName)
                            _logger.LogInformation(
                                $"No newer version is available for iOS(current version is {version.VersionName}), check update for iOS ok.");
                        var result = await _data.CreateVersionAsync(version);
                        if (result != 1) throw new Exception("Unknown Error");
                        else
                            _logger.LogInformation(
                                $"Found new version for iOS: {version.VersionName}, check update for iOS ok");
                    }
                }
                else
                {
                    if (os == OS.Android)
                    {
                        var url = "https://api.github.com/repos/UNIDY2002/THUInfo/releases/latest";
                        var content = await _client.GetStringAsync(url);
                        var json = JsonNode.Parse(content);
                        var versionName = (string)json["name"];
                        if (versionName == _currentVersionOfAndroid.VersionName)
                            _logger.LogInformation(
                                $"No newer version is available for Android(current version is {versionName}), check update for Android ok.");
                        else
                        {
                            var publishedAt = DateTime.Parse((string)json["published_at"]).ToLocalTime();
                            var releaseNote = (string)json["body"];
                            var version = new Version()
                            {
                                CreatedTime = publishedAt,
                                IsAndroid = true,
                                ReleaseNote = releaseNote,
                                VersionName = versionName
                            };
                            var result = await _data.CreateVersionAsync(version);
                            if (result != 1) throw new Exception("Unknown Error");
                            else
                                _logger.LogInformation(
                                    $"Found new version for Android: {versionName}, check update ok");
                        }
                    }
                    else // handle ios
                    {
                        var url = "https://itunes.apple.com/lookup?id=1533968428";
                        var content = await _client.GetStringAsync(url);
                        var json = JsonNode.Parse(content)["results"].AsArray()[0];
                        var versionName = (string)json["version"];
                        if (versionName == _currentVersionOfIOS.VersionName)
                            _logger.LogInformation(
                                $"No newer version is available for iOS(current version is {versionName}), check update for iOS ok.");
                        else
                        {
                            var publishedAt = DateTime.Parse((string)json["currentVersionReleaseDate"]).ToLocalTime();
                            var releaseNote = (string)json["releaseNotes"];
                            var version = new Version()
                            {
                                CreatedTime = publishedAt,
                                IsAndroid = false,
                                ReleaseNote = releaseNote,
                                VersionName = versionName
                            };
                            var result = await _data.CreateVersionAsync(version);
                            if (result != 1) throw new Exception("Unknown Error");
                            else
                                _logger.LogInformation(
                                    $"Found new version for iOS: {versionName}, check update for iOS ok");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Checking update for {os} failed.");
            }
            finally
            {
                var version = await _data.GetVersionAsync(os == OS.Android);
                if (os == OS.Android) _currentVersionOfAndroid = version;
                else _currentVersionOfIOS = version;
                IsRunning = false;
            }
        }
        public enum OS
        {
            Android,
            IOS
        }
    }
}
