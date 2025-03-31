using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookShop.Models
{
    public class BookShopUser: IdentityUser
    {
        public BookShopRole Role { get; set; }

        [Required(ErrorMessage ="Please write your address")]
        public string AddressLine { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
       
      
    }
    public enum BookShopRole
    { 
        User,
        Admin
    }



}
