using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models
{
    public class RegisterViewModel
    {
        [Required, Display(Name = "用户名")]
        public string Name { get; set; }
        [Required, DataType(DataType.Password), Display(Name = "密码"),RegularExpression("^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,20}$", ErrorMessage ="密码必须并只能包含字母和数字，长度为6到20位")]
        public string Password { get; set; }
    }
}
