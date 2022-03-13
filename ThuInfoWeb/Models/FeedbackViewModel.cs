using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models
{
    public class FeedbackViewModel
    {
        [Column(IsIdentity = true,IsPrimary = true)]
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string OS { get; set; }
        [Required]
        public string AppVersion { get; set; }
        public DateTime CreatedTime { get; set; }
        public string? Reply { get; set; }
    }
}
