using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Dtos
{
    public class FeedbackDto
    {
        [Required]
        public string Content { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        [Required]
        public string OS { get; set; }
        [Required]
        public string AppVersion { get; set; }
        [Required]
        public string PhoneModel { get; set; }
        
    }
}
