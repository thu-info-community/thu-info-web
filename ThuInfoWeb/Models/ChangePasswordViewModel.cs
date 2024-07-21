using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models;

public class ChangePasswordViewModel
{
    [Required]
    [Display(Name = "用户名")]
    public string? Name { get; init; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "旧密码")]
    public string? OldPassword { get; init; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "新密码")]
    [RegularExpression("^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,20}$", ErrorMessage = "密码必须并只能包含字母和数字，长度为6到20位")]
    public string? NewPassword { get; init; }
}
