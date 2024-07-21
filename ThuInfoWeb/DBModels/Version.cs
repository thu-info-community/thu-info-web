using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

public class Version
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; init; }

    [Column(IsNullable = false)]
    public string VersionName { get; init; } = string.Empty;

    [Column(StringLength = -1, IsNullable = false)]
    public string ReleaseNote { get; init; } = string.Empty;

    [Column(IsNullable = false)]
    public DateTime CreatedTime { get; init; }

    [Column(IsNullable = false)]
    public bool IsAndroid { get; init; }
}
