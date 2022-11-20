using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

public class Startup
{
    [Column(IsPrimary = true,IsIdentity = true)]
    public int Id { get; set; }

    public DateTime CreatedTime { get; set; }
}