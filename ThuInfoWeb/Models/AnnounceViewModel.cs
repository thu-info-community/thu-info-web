using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models;

public class AnnounceViewModel
{
    [Required]
    [Display(Name = "标题")]
    public string? Title { get; init; }

    [Required]
    [Display(Name = "内容")]
    public string? Content { get; init; }

    public int Id { get; init; }

    public string? Author { get; init; }

    public DateTime CreatedTime { get; init; }

    public bool IsActive { get; init; } = true;

    [Display(Name = "对此版本及之前版本可见")]
    public string? VisibleNotAfter { get; init; }

    [Display(Name = "对这些版本可见 (使用逗号分隔)")]
    public string? VisibleExact { get; init; }
}
