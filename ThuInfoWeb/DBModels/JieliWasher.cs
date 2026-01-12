using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

[Index("ix_jieliwashers_building", nameof(Building), false)]
public class JieliWasher
{
    [Column(IsPrimary = true, StringLength = 20, IsNullable = false)]
    public string Id { get; set; } = string.Empty;

    [Column(StringLength = 20, IsNullable = false)]
    public string Building { get; set; } = string.Empty;

    [Column(StringLength = 20, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public DateTime CreatedTime { get; set; }
}
