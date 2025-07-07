using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using ProductApp.Models;
using ProductApp.Repositories;
using ProductApp.Identity;
using ProductApp.Data;
using System.Data;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnection>(sp =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserStore<ApplicationUser>, DapperUserStore>();
builder.Services.AddScoped<IRoleStore<ApplicationRole>, DapperRoleStore>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<OrderRepository>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = false;
    opts.Password.RequiredLength = 6;
    opts.Password.RequireDigit = true;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireNonAlphanumeric = true;
})
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

//seeding database (creating defaults before loading website )
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");

        if (app.Environment.IsDevelopment())
        {
            throw;
        }
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();