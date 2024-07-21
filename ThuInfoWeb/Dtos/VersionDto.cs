namespace ThuInfoWeb.Dtos;

public class VersionDto
{
    public string VersionName { get; set; } = string.Empty;

    public string ReleaseNote { get; set; } = string.Empty;

    public DateTime CreatedTime { get; set; }

    public string DownloadUrl { get; set; } = string.Empty;
}
