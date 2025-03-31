using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BookShop.Models;
using BookShop.Repository;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BookShop.AppDbContext;

namespace BookShop.Controllers
{
    public class HomeController(IBookRepository bookRepository,
                                ILogger<HomeController> logger, 
                                UserManager<BookShopUser> userManager,
                                AppIdentityDbContext dbContext) : Controller
    {
        private readonly ILogger<HomeController> _logger= logger;
        private readonly IBookRepository _bookRepository= bookRepository;
        private readonly UserManager<BookShopUser> _userManager= userManager;
        private readonly AppIdentityDbContext _dbContext = dbContext;

      

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Home page Visited");
            var listOfBooks = await _bookRepository.GetAllAsync();
            return View(listOfBooks);
        }

        public async Task<IActionResult> ViewBookDetails(int bookId)
        {
            var bookDetail = await _bookRepository.GetByIdAsync(bookId);
            return View(bookDetail);
        }
        public async Task<IActionResult> Search(string bookName)
        {
            if (string.IsNullOrEmpty(bookName))
            {
                return View("Index", null);
            }

            var books = await _bookRepository.GetAllAsync();
            books = books.Where(b => b.Title.Contains(bookName, StringComparison.OrdinalIgnoreCase)).ToList();

            return View("Index", books);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}