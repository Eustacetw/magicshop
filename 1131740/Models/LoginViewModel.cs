using System.ComponentModel.DataAnnotations;

namespace _1131740.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        public string Username { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}