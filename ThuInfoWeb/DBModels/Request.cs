using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

public class Request
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    [Column(IsNullable = false)]
    public string Method { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public string Path { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public DateTime Time { get; set; }

    [Column(IsNullable = false)]
    public uint Ip { get; set; }
}
