using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? BookName { get; set; }
        public double Price { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public BookShopUser? User { get; set; }
    
        public int Quantity {get; set; }
    }
}
