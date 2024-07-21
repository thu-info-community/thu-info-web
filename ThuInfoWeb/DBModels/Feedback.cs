using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

public class Feedback
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }

    [Column(StringLength = -1, IsNullable = false)]
    public string Content { get; init; } = string.Empty;

    [Column(IsNullable = false)]
    public string Contact { get; init; } = string.Empty;

    [Column(IsNullable = false)]
    public DateTime CreatedTime { get; init; }

    [Column(IsNullable = false)]
    public string AppVersion { get; init; } = "0.0.0";

    [Column(IsNullable = false)]
    public string OS { get; init; } = string.Empty;

    [Column(IsNullable = false)]
    public string PhoneModel { get; init; } = string.Empty;

    [Column(IsNullable = false)]
    public string Reply { get; init; } = string.Empty;

    [Column(IsNullable = false)]
    public string ReplierName { get; init; } = string.Empty;

    [Column(IsNullable = false)]
    public DateTime? RepliedTime { get; init; }
}
