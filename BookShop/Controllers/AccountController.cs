using BookShop.AppDbContext;
using BookShop.Models;
using BookShop.Models.AccountViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    public class AccountController(UserManager<BookShopUser> userManager,AppIdentityDbContext dbContext) : Controller
    {
        private readonly UserManager<BookShopUser> _userManager = userManager;
        private readonly AppIdentityDbContext _dbContext= dbContext;
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            if (!registerViewModel.Password.Equals(registerViewModel.ConfirmPassword))
            {
                ModelState.AddModelError("Password", "Passwords do not match.");
                return View(registerViewModel);
            }

            var blockedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerViewModel.Email && u.IsBlocked);
            if (blockedUser != null)
            {
                ModelState.AddModelError(string.Empty, "This user is blocked by admin.");
                return View(registerViewModel);
            }

            BookShopUser user = new()
            {
                UserName = registerViewModel.Email,
                Email = registerViewModel.Email,
                Role = BookShopRole.User,
                AddressLine = registerViewModel.AddressLine,
                IsBlocked=false
            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registerViewModel);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, registerViewModel.FullName),
                new Claim(ClaimTypes.Email, registerViewModel.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, "User")
            };

            var claimResult = await _userManager.AddClaimsAsync(user, claims);
            await _userManager.UpdateAsync(user);
            if (!claimResult.Succeeded)
            {
                foreach (var error in claimResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registerViewModel);
            }

            return View("Success");
        }

        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl = "/")
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            var blockedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginViewModel.Email && u.IsBlocked);
            if (blockedUser != null)
            {
                ModelState.AddModelError(string.Empty, "This user is blocked by admin.");
                return View(loginViewModel);
            }

            var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "This email is not registered.");
                return View(loginViewModel);
            }

            var result = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(loginViewModel);
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var scheme = IdentityConstants.ApplicationScheme;
            var claimIdentity = new ClaimsIdentity(claims, scheme);
            var principal = new ClaimsPrincipal(claimIdentity);
            var authenticationProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20)
            };

            await HttpContext.SignInAsync(scheme, principal, authenticationProperties);

            return Redirect(returnUrl);
        }
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Redirect("/Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

    }

}

