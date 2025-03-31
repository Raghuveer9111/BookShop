using System.ComponentModel.DataAnnotations;

namespace BookShop.Models.AccountViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please Enter your FullName")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is mandatory")]
        [EmailAddress(ErrorMessage = "This is not a valid email, use this format test@test.com")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a password a password to secure your account")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "This password has to match the previous one")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is Required")]
        public string AddressLine { get; set; } = string.Empty;
    }
}
