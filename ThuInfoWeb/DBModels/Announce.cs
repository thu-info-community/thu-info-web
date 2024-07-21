using System.Text.Json.Serialization;
using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

public class Announce
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; init; }

    [Column(StringLength = 50, IsNullable = false)]
    public string Title { get; init; } = string.Empty;

    [Column(StringLength = -1, IsNullable = false)]
    public string Content { get; init; } = string.Empty;

    [Column(StringLength = 50, IsNullable = false)]
    public string Author { get; init; } = string.Empty;

    [Column(IsNullable = false)]
    public DateTime CreatedTime { get; init; }

    [JsonIgnore]
    [Column(IsNullable = false)]
    public bool IsActive { get; init; }

    [Column(StringLength = 10, IsNullable = false)]
    public string VisibleNotAfter { get; init; } = "9.9.9";

    [Column(StringLength = 30, IsNullable = false)]
    public string VisibleExact { get; init; } = "";
}
