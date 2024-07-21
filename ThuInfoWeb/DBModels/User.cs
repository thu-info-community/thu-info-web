using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

public class User
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    [Column(IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public bool IsAdmin { get; set; }

    [Column(IsNullable = false)]
    public DateTime CreatedTime { get; set; }
}
