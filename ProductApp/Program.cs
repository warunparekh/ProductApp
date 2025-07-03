using MySqlConnector;
using System.Data;
using ProductApp.Repositories;
using ProductApp.Services;

namespace ProductApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "ProductApp.Session";
            });

            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<ProductRepository>();
            builder.Services.AddScoped<CategoryRepository>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<DatabaseSeeder>();

            builder.Services.AddScoped<IDbConnection>(provider =>
                new MySqlConnection("Server=localhost;Database=productapp;User=kirito;Password=admin;"));

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                try
                {
                    await seeder.SeedAsync();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "admin",
                pattern: "Admin/{action=Index}/{id?}",
                defaults: new { controller = "Admin" });

            app.MapControllerRoute(
                name: "account",
                pattern: "Account/{action=Login}",
                defaults: new { controller = "Account" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}