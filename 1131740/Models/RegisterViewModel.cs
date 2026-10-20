using System.ComponentModel.DataAnnotations;

namespace _1131740.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        public string Username { get; set; }

        [Required(ErrorMessage = "請輸入真實姓名")]
        public string Name { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "請再次輸入密碼")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "兩次輸入的密碼不一致！")] 
        public string ConfirmPassword { get; set; }
    }
}