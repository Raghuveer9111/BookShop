using BookShop.AppDbContext;
using BookShop.Models;
using BookShop.Models.UserViewModel;
using BookShop.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookShop.Controllers
{
    [Authorize(Policy ="MustbeUser")]
    public class UserController(UserManager<BookShopUser> userManager, 
                                AppIdentityDbContext dbContext,
                                IBookRepository bookRepository,
                                ICartRepository cardItemRepository) : Controller
    {
        private readonly UserManager<BookShopUser> _userManager = userManager;
        private readonly AppIdentityDbContext _dbContext = dbContext;
        private readonly IBookRepository _bookRepository = bookRepository;
        private readonly ICartRepository _cardItemRepository = cardItemRepository;
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CartView()
        {
            var loggedInUserEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (loggedInUserEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.FindByEmailAsync(loggedInUserEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _cardItemRepository.GetAllAsync();
            cartItems = cartItems.Where(x => x.UserId == user.Id).ToList();
            var model = new HomeViewModel
            {
               CartItems = (List<CartItem>)cartItems
            };
            

            return View(model);
        }
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            var cartItem = await _cardItemRepository.GetByIdAsync(bookId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    BookId = book.BookId,
                    BookName = book.BookName,
                    Title = book.Title,
                    Price = book.Price,
                    Quantity = 1,
                    UserId = user.Id
                };
                await _cardItemRepository.AddAsync(cartItem);
            }
            else
            {
                cartItem.Quantity++;
                await _cardItemRepository.UpdateAsync(cartItem);
            }

            return RedirectToAction("Index","Home");
        }
        [HttpPost]
        public async Task<IActionResult> RemoveFromCard(int bookId)
        {
            var loggedInUserEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (loggedInUserEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.FindByEmailAsync(loggedInUserEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = (await _cardItemRepository.GetAllAsync()).FirstOrDefault(ci => ci.BookId == bookId && ci.UserId == user.Id);
            if (cartItem == null)
            {
                return NotFound();
            }

            if (cartItem.Quantity <= 1)
            {
                await _cardItemRepository.DeleteAsync(cartItem.Id);
            }
            else
            {
                cartItem.Quantity--;
                await _cardItemRepository.UpdateAsync(cartItem);
            }

            return RedirectToAction("CartView");
        }
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var loggedInUserEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (loggedInUserEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.FindByEmailAsync(loggedInUserEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = (await _cardItemRepository.GetAllAsync()).Where(ci => ci.UserId == user.Id).ToList();
            if (!cartItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Your cart is empty.");
                return View("CartView", new HomeViewModel { CartItems = cartItems });
            }

            foreach (var cartItem in cartItems)
            {
                var book = await _bookRepository.GetByIdAsync(cartItem.BookId);
                if (book == null || book.Quantity < cartItem.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"The book '{cartItem.BookName}' is out of stock or does not have enough quantity.");
                    return View("CartView", new HomeViewModel { CartItems = cartItems });
                }
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(ci => ci.Price * ci.Quantity),
                User = user,
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    BookId = ci.BookId,
                    BookName = ci.BookName,
                    Price = ci.Price,
                    Quantity = ci.Quantity
                }).ToList()
            };

            await _dbContext.Orders.AddAsync(order);

            foreach (var cartItem in cartItems)
            {
                var book = await _bookRepository.GetByIdAsync(cartItem.BookId);
                if (book != null)
                {
                    book.Quantity -= cartItem.Quantity;
                    await _bookRepository.UpdateAsync(book);
                }
            }

            _dbContext.CartItems.RemoveRange(cartItems);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ConfirmOrder", order);
        }
        public IActionResult ConfirmOrder(Order order)
        {
            return View(order);
        }

        public async Task<IActionResult> UserProfile()
        {
            var loggedInUserEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (loggedInUserEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.FindByEmailAsync(loggedInUserEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var claim = await _userManager.GetClaimsAsync(user);

            var model = new UserProfileViewModel
            {
                Email = user.Email,
                UserName = claim.FirstOrDefault(x => x.Type==ClaimTypes.Name)?.Value?? string.Empty,
                AddressLine = user.AddressLine
            };

            return View(model);
        }

        public async Task<IActionResult> EditProfile(UserProfileViewModel model)
        {
            var loggedInUserEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (loggedInUserEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.FindByEmailAsync(loggedInUserEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
                user.AddressLine = model.AddressLine ?? " ";

                var claims = await _userManager.GetClaimsAsync(user);
                var removeResult = await _userManager.RemoveClaimsAsync(user, claims);

                if (!removeResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Unable to update claim - removing existing claim failed");
                    return View("UserProfile", model);
                }

                var claimsRequired = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.UserName ?? " "),
                    new Claim(ClaimTypes.Role, "User"),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, model?.Email ?? string.Empty)
                };
                var addClaimResult = await _userManager.AddClaimsAsync(user, claimsRequired);
                if (!addClaimResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Unable to update claim - adding new claim failed");
                    return View("UserProfile", model);
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Change password if provided and if new password matches confirm password
                    if (!string.IsNullOrEmpty(model?.NewPassword) && !string.IsNullOrEmpty(model.ConfirmNewPassword))
                    {
                        if (model.NewPassword == model.ConfirmNewPassword)
                        {
                            if (!string.IsNullOrEmpty(model.CurrentPassword))
                            {
                                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                                if (!passwordChangeResult.Succeeded)
                                {
                                    foreach (var error in passwordChangeResult.Errors)
                                    {
                                        ModelState.AddModelError(string.Empty, error.Description);
                                    }

                                    return View("UserProfile", model);
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Current password is required to change the password.");
                                return View("UserProfile", model);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "The new password and confirm password do not match.");
                            return View("UserProfile", model);
                        }
                    }
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    return View("UserProfile", model);
                }
            }
            return View("UserProfile", model);
        }

        public async Task<IActionResult> ViewOrders()
        {
            var loggedInUserEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (loggedInUserEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.FindByEmailAsync(loggedInUserEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == user.Id)
                .ToListAsync();

            var model = new UserOrderViewModel
            {
                Orders = orders
            };

            return View(model);
        }

    }
}

