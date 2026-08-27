using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models;

public class MiscViewModel
{
    [Required]
    [Url]
    public string? QrCodeContent { get; init; }

    [Required]
    public int CardIVersion { get; init; }

    [Required]
    public int SchoolCalendarYear { get; init; }
}
