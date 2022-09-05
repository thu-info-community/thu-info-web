using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models
{
    public class AnnounceViewModel
    {
        [Required, Display(Name = "标题")]
        public string Title { get; set; }
        [Required, Display(Name = "内容")]
        public string Content { get; set; }
        public int Id { get; set; }
        public string Author { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
