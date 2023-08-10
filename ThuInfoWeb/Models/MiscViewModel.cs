using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models
{
    public class MiscViewModel
    {
        [Required, Url]
        public string QrCodeContent { get; set; }
        [Required, Url]
        public string ApkUrl { get; set; }
        [Required]
        public int CardIVersion { get; set; }
    }
}
