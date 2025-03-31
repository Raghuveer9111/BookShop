using System.ComponentModel.DataAnnotations;

namespace BookShop.Models.AccountViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is mandatory")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is mandatory")]
        public string Password { get; set; } = string.Empty;
    }
}
