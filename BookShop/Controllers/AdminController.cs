using BookShop.AppDbContext;
using BookShop.Models;
using BookShop.Models.AccountViewModel;
using BookShop.Models.UserViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    [Authorize(Policy ="MustbeAdmin")]
    public class AdminController (UserManager<BookShopUser> userManager,AppIdentityDbContext DbContext): Controller
    {
        public readonly UserManager<BookShopUser> _userManager= userManager;
        private readonly AppIdentityDbContext _dbContext = DbContext;
        public IActionResult Index()
        {
            return View();
        }
        public async Task<string> GetUserNameAsync(string email)
        {
            var accountuser = await _userManager.FindByEmailAsync(email);
            if(accountuser!=null)
            {
                var claim = await _userManager.GetClaimsAsync(accountuser);

                if(claim!= null)
                {
                    return claim.FirstOrDefault(x => x.Type==ClaimTypes.Name)?.Value?? string.Empty;
                }
            }
            return string.Empty;
        }
        private async Task<List<UserViewModel>> GetUsersToManageAsync()
        {
            var users = await _userManager.Users
                .Where(x => x.Role != BookShopRole.Admin && !x.IsBlocked)
                .ToListAsync();

            var listOfUserAccount = new List<UserViewModel>();
            foreach (var user in users)
            {
                listOfUserAccount.Add(new UserViewModel
                {
                    Email = user.Email,
                    Name = await GetUserNameAsync(user.Email ?? string.Empty),
                    Role = user.Role,
                    AddressLine = user.AddressLine,
                });
            }

            return listOfUserAccount;
        }
        public async Task<IActionResult> ManageUsers()
        {
            return View(await GetUsersToManageAsync());
        }

        public async Task<IActionResult> EditUser(string email)
        {
            var accountUser = await _userManager.FindByEmailAsync(email);

            if(accountUser!= null)
            {
                UserViewModel userViewModel = new UserViewModel()
                {
                    Email=accountUser.Email,
                    Name=await GetUserNameAsync(accountUser.Email?? string.Empty),
                    Role=accountUser.Role,
                    AddressLine= accountUser.AddressLine,
                };
                return View(userViewModel);

            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserViewModel userViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userViewModel);
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(userViewModel.Email))
                    {
                        BookShopUser? bookshopUser = await _userManager.FindByEmailAsync(userViewModel.Email);
                        if (bookshopUser != null)
                        {
                            bookshopUser.Role = userViewModel.Role;
                            bookshopUser.AddressLine=userViewModel.AddressLine?? string.Empty;
                            
                            var claim = await _userManager.GetClaimsAsync(bookshopUser);
                            var removeResult = await _userManager.RemoveClaimsAsync(bookshopUser, claim);

                            if (!removeResult.Succeeded)
                            {
                                ModelState.AddModelError(string.Empty, "Unable to Update claim - removing existing claim");
                                return View(userViewModel);
                            }

                            var claimsRequired = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, userViewModel.Name ?? " "),
                                new Claim(ClaimTypes.Role, Enum.GetName(typeof(BookShopRole), userViewModel.Role) ?? " "),
                                new Claim(ClaimTypes.NameIdentifier, bookshopUser.Id),
                                new Claim(ClaimTypes.Email, userViewModel.Email)
                            };
                            var addClaimResult = await _userManager.AddClaimsAsync(bookshopUser, claimsRequired);
                            if (!addClaimResult.Succeeded)
                            {
                                ModelState.AddModelError(string.Empty, "Unable to update claim - adding claim failed");
                                return View(userViewModel);
                            }

                            var userUpdateResult = await _userManager.UpdateAsync(bookshopUser);
                            if (!userUpdateResult.Succeeded)
                            {
                                ModelState.AddModelError(string.Empty, "Failed to update user");
                                return View(userViewModel);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(userViewModel);
                }
            }

            return RedirectToAction("ManageUsers", await GetUsersToManageAsync());
        }

        public async Task<IActionResult> DeleteUser(string email)
        {
            var accountUser = await _userManager.FindByEmailAsync(email);
            
            if(accountUser != null)
            {
                await _userManager.DeleteAsync(accountUser);
                return View("ManageUsers", await GetUsersToManageAsync());
            }
            return NotFound();
        }

        public IActionResult CreateUser()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel createUserViewModel)
        {
            if(!ModelState.IsValid)
            {
                return View(createUserViewModel);
            }

            if(createUserViewModel.Email != createUserViewModel.ConfirmEmail )
            {
                ModelState.AddModelError(string.Empty, "Email and confirmEmail do not match");
                return View(createUserViewModel);
            }
            if(createUserViewModel.Password != createUserViewModel.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Paasword and confirmPassword do not match");
                return View(createUserViewModel);
            }
            BookShopUser bookShopUser = new()
            { 
                Email= createUserViewModel.Email,
                UserName=createUserViewModel.Name,
                Role=createUserViewModel.Role,
                AddressLine=createUserViewModel?.AddressLine??" "
            };

            var createUser = await _userManager.CreateAsync(bookShopUser, createUserViewModel?.Password?? " ");

            if (!createUser.Succeeded)
            {
                foreach(var error in createUser.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(createUserViewModel);
            }

            var claimRequired = new List<Claim>
            {
                new Claim(ClaimTypes.Name, createUserViewModel?.Name ?? " "),
                new Claim(ClaimTypes.Role, Enum.GetName(createUserViewModel.Role) ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, bookShopUser.Id),
                new Claim(ClaimTypes.Email, createUserViewModel.Email ?? " ")
            };

            var claimResult = await _userManager.AddClaimsAsync(bookShopUser, claimRequired);
            if(!claimResult.Succeeded)
            {
                foreach(var error in claimResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(createUserViewModel);
            }

            return RedirectToAction("ManageUsers", await GetUsersToManageAsync());
        }
        public async Task<IActionResult> ManageBookStock()
        {
            try
            {
                var listOfBook = await _dbContext.Books.ToListAsync();
                return View(listOfBook);
            }
            catch (Exception ex)
            {
                // Log the exception (use a logging framework or simply write to the console for debugging)
                Console.WriteLine(ex.Message);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
        public IActionResult AddBook()
        {
            return View(new Book());
        }
        [HttpPost]
        public async Task<IActionResult> AddBook(Book book, IFormFile CoverImage)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            if(CoverImage!= null)
            {
                using var memoryStream = new MemoryStream();
                await CoverImage.CopyToAsync(memoryStream);
                book.CoverImage = memoryStream.ToArray();
            }

            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ManageBookStock");
        }
        public async Task<IActionResult> EditBookDetails(int BookId)
        {
            var book = await _dbContext.Books.FindAsync(BookId);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> EditBookDetails(Book book, IFormFile? CoverImage)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            var existingBook = await _dbContext.Books.FindAsync(book.BookId);
            if (existingBook == null)
            {
                return NotFound();
            }

            existingBook.BookName = book.BookName;
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.ISBN = book.ISBN;
            existingBook.PublishedDate = book.PublishedDate;
            existingBook.Price = book.Price;
            existingBook.Genre = book.Genre;
            existingBook.Description = book.Description;
            existingBook.Publisher = book.Publisher;
            existingBook.Quantity = book.Quantity;
            existingBook.BookName=book.BookName;

            if (CoverImage != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await CoverImage.CopyToAsync(memoryStream);
                    existingBook.CoverImage = memoryStream.ToArray();
                }
            }

            _dbContext.Update(existingBook);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ManageBookStock");
        }
        public async Task<IActionResult> DeleteBook(int BookId)
        {
            var book = await _dbContext.Books.FindAsync(BookId);
            if (book == null)
            {
                return NotFound();
            }

            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ManageBookStock");
        }

        public async Task<IActionResult> BlockUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Mark user as blocked
                user.IsBlocked = true;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Failed to block user.");
                    return View("ManageUsers", await GetUsersToManageAsync());
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("ManageUsers", await GetUsersToManageAsync());
            }

            return RedirectToAction("ManageUsers");
        }
        public async Task<IActionResult> BlockUserDetails()
        {
            var blockedUserDetails = await _dbContext.Users
                .Where(u => u.IsBlocked)
                .ToListAsync();

            if (blockedUserDetails != null && blockedUserDetails.Any())
            {
                return View(blockedUserDetails);
            }
            else
            {
                return View(new BookShopUser());
            }
        }
        public async Task<IActionResult> UnblockUser(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email && x.IsBlocked);
            if (user != null)
            {
                user.IsBlocked = false;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("BlockUserDetails");
            }

            return NotFound();
        }
        public async Task<IActionResult> ViewAllOrderItems()
        {
            var orders = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .ToListAsync();

            var model = new UserOrderViewModel
            {
                Orders = orders
            };

            return View(model);
        }

    }
}
