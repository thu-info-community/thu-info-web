using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Dtos
{
    public class VersionDto
    {
        [Required]
        public string VersionName { get; set; }
        [Required]
        public string ReleaseNote { get; set; }
        public DateTime CreatedTime { get; set; }
        [Required]
        public bool IsAndroid { get; set; }
    }
}
