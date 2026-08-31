namespace ThuInfoWeb;

internal static partial class ApplicationLog
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Unable to migrate the password hash for user {UserName}.")]
    internal static partial void PasswordHashMigrationFailed(ILogger logger, string userName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unable to persist the HTTP request log for {Path}.")]
    internal static partial void HttpRequestLogPersistenceFailed(ILogger logger, Exception exception, string path);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unable to initialize cached application versions.")]
    internal static partial void VersionCacheInitializationFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Start checking update for {OS}, current version is {Version}.")]
    internal static partial void VersionCheckStarted(ILogger logger, string os, string version);

    [LoggerMessage(Level = LogLevel.Information, Message = "No newer version is available for {OS}.")]
    internal static partial void NoNewerVersion(ILogger logger, string os);

    [LoggerMessage(Level = LogLevel.Information, Message = "Found new version for {OS}: {VersionName}.")]
    internal static partial void VersionFound(ILogger logger, string os, string versionName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Version check for {OS} was canceled.")]
    internal static partial void VersionCheckCanceled(ILogger logger, string os);

    [LoggerMessage(Level = LogLevel.Error, Message = "Checking update for {OS} failed.")]
    internal static partial void VersionCheckFailed(ILogger logger, Exception exception, string os);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unable to refresh cached version for {OS}.")]
    internal static partial void VersionCacheRefreshFailed(ILogger logger, Exception exception, string os);

    [LoggerMessage(Level = LogLevel.Warning, Message = "APK for Android version {VersionName} was not found at {ApkPath}; the version will not be saved.")]
    internal static partial void AndroidApkMissing(ILogger logger, string versionName, string apkPath);
}
