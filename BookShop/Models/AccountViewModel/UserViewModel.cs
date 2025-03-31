using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShop.Models.AccountViewModel
{
    public class UserViewModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public BookShopRole Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressLine { get; set; }
       
    }
}
