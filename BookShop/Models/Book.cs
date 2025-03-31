using System.ComponentModel.DataAnnotations;

namespace BookShop.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Please provide Book Name")]
        public string BookName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a title.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter author name.")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        public string ISBN { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Title is required")]

        public string Genre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Publisher { get; set; }

        [Required(ErrorMessage = "The CoverImage field is required.")]
        public byte[] CoverImage { get; set; } = Array.Empty<byte>();

        [Required(ErrorMessage = "Please mention Number of Copy's of this book")]
        public int Quantity { get; set; }
    }
}
