using BookShop.AppDbContext;
using BookShop.Models;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Repository
{
    public class BookRepository(AppIdentityDbContext dbContext) : IBookRepository
    {
        private readonly AppIdentityDbContext _dbContext = dbContext;

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _dbContext.Books.ToListAsync();
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            return await _dbContext.Books.FindAsync(id);
        }

        public async Task AddAsync(Book book)
        {
            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _dbContext.Books.Update(book);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _dbContext.Books.FindAsync(id);
            if (book != null)
            {
                _dbContext.Books.Remove(book);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
