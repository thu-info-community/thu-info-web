using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models;

public class LoginViewModel
{
    [DataType(DataType.Text)]
    [Display(Name = "用户名")]
    [Required]
    [MaxLength(100)]
    public string? Name { get; init; }

    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    [Required]
    [MaxLength(20)]
    public string? Password { get; init; }
}
