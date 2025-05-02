using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

public class Startup
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; init; }

    [Column(IsNullable = false)]
    public DateTime CreatedTime { get; init; }

    [Column(IsNullable = true)]
    public Guid? Uuid { get; set; }
}
