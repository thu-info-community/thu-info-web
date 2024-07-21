using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThuInfoWeb.Dtos;

public class SocketDto
{
    [Required]
    public int? SeatId { get; init; }

    [Required]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool? IsAvailable { get; set; }

    public int SectionId { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
    public string? Status { get; set; }
}
