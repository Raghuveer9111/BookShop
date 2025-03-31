using BookShop.AppDbContext;
using BookShop.Models;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Repository
{
    public class CartRepository(AppIdentityDbContext dbContext) : ICartRepository
    {
        private readonly AppIdentityDbContext _dbContext=dbContext;


        public async Task<IEnumerable<CartItem>> GetAllAsync()
        {
            return await _dbContext.CartItems.ToListAsync();
        }

        public async Task<CartItem> GetByIdAsync(int id)
        {
            return await _dbContext.CartItems.FindAsync(id);
        }

        public async Task AddAsync(CartItem cartItem)
        {
            await _dbContext.CartItems.AddAsync(cartItem);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(CartItem cartItem)
        {
            _dbContext.CartItems.Update(cartItem);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cartItem = await _dbContext.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _dbContext.CartItems.Remove(cartItem);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
