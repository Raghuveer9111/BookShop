using BookShop.AppDbContext;
using BookShop.Models;
using BookShop.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddIdentity<BookShopUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit=true;
                options.Password.RequireLowercase=true;
                options.Password.RequireUppercase=true;
                options.Password.RequiredLength=6;

                options.Lockout=new LockoutOptions
                {
                    AllowedForNewUsers=true,
                    DefaultLockoutTimeSpan=TimeSpan.FromMinutes(5),
                    MaxFailedAccessAttempts=5
                };
                options.User.RequireUniqueEmail= true;

            }).AddEntityFrameworkStores<AppIdentityDbContext>()
               .AddDefaultTokenProviders();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppIdentityDbContext>(options =>
            options
            .UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme= IdentityConstants.ApplicationScheme;
            }).AddCookie();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath="/Account/Login";
                options.AccessDeniedPath="/Account/AccessDenied";
            });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("MustbeAdmin", policy =>
                                                 policy
                                                 .RequireClaim(ClaimTypes.Role, "Admin")
                                                 .RequireClaim(ClaimTypes.Role, "User"))
                .AddPolicy("MustbeUser", policy => policy.RequireClaim(ClaimTypes.Role, "User"));

            builder.Services.AddScoped<IBookRepository, BookRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
