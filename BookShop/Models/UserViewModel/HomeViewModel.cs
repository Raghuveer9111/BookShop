namespace BookShop.Models.UserViewModel
{
    public class HomeViewModel
    {
        public List<Book> Books { get; set; } = new List<Book>();
        public List<CartItem>? CartItems { get; set; }
    }
}
