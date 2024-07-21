using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models;

public class UserViewModel
{
    [Required]
    public string? Name { get; set; }

    [Required]
    public bool IsAdmin { get; set; } = false;
}
