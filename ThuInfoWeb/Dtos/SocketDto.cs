using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Dtos
{
    public class SocketDto
    {
        [Required]
        public int SeatId { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
    }
}
