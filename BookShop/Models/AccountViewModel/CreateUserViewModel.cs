namespace BookShop.Models.AccountViewModel
{
    public class CreateUserViewModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ConfirmEmail { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? AddressLine { get; set; }
        public BookShopRole Role { get; set; }
    }
}
