using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookShop.Models
{
    public class Order
    {
            [Key]
            public int OrderId { get; set; }
            public string? UserId { get; set; }
            public DateTime OrderDate { get; set; }
            public double TotalAmount { get; set; }
            public ICollection<OrderItem> OrderItems { get; set; }= new List<OrderItem>();

        [ForeignKey("UserId")]
            public BookShopUser? User { get; set; }
    }
}
