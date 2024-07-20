using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models
{
    public class AnnounceViewModel
    {
        [Required, Display(Name = "标题")]
        public string? Title { get; set; }

        [Required, Display(Name = "内容")]
        public string? Content { get; set; }

        public int Id { get; set; }

        public string? Author { get; set; }

        public DateTime CreatedTime { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "对此版本及之前版本可见")]
        public string? VisibleNotAfter { get; set; }

        [Display(Name = "对这些版本可见 (使用逗号分隔)")]
        public string? VisibleExact { get; set; }
    }
}
