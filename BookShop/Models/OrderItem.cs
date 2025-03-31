using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookShop.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int? BookId { get; set; }
        public string? BookName { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }


    }
}
