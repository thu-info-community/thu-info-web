using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Dtos
{
    public class LostAndFoundDto
    {
        public int Id { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public int SenderId { get; set; }
        public int? TargetId { get; set; }
    }
}
