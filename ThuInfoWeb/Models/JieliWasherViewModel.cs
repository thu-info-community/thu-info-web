using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models;

public class JieliWasherViewModel
{
    [Required]
    [StringLength(20)]
    [Display(Name = "ID")]
    public string? Id { get; init; }

    [Required]
    [StringLength(20)]
    [Display(Name = "楼号")]
    public string? Building { get; init; }

    [Required]
    [StringLength(20)]
    [Display(Name = "名称")]
    public string? Name { get; init; }

    public DateTime CreatedTime { get; init; }
}
