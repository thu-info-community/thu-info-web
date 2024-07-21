using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Dtos;

public class FeedbackDto
{
    [Required]
    public string Content { get; init; } = string.Empty;

    [MaxLength(256)]
    public string Contact { get; init; } = string.Empty;

    [Required]
    public string OS { get; init; } = string.Empty;

    [Required]
    public string AppVersion { get; init; } = string.Empty;

    [Required]
    public string PhoneModel { get; init; } = string.Empty;
}
