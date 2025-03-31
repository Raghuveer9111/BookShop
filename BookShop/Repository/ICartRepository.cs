using BookShop.Models;

namespace BookShop.Repository
{
    public interface ICartRepository
    {
        Task<IEnumerable<CartItem>> GetAllAsync();
        Task<CartItem> GetByIdAsync(int id);
        Task AddAsync(CartItem cartItem);
        Task UpdateAsync(CartItem cartItem);
        Task DeleteAsync(int id);
    }
}
