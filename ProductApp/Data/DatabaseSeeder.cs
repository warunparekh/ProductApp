using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ProductApp.Models;

namespace ProductApp.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            try
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                Console.WriteLine("Starting database seeding...");

                await SeedRolesAsync(roleManager);

                await SeedAdminUserAsync(userManager);

                Console.WriteLine("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database seeding: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            var roles = new[] { "Admin", "User" };

            foreach (var roleName in roles)
            {
                try
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var role = new ApplicationRole
                        {
                            Name = roleName,
                            NormalizedName = roleName.ToUpper()
                        };

                        var result = await roleManager.CreateAsync(role);
                        if (result.Succeeded)
                        {
                            Console.WriteLine($"Created role: {roleName}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Role {roleName} already exists");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating role {roleName}: {ex.Message}");
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@productapp.com";
            const string adminPassword = "Admin@123456";

            try
            {
                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin == null)
                {
                    var admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        IsAdmin = true,
                        UserAddress = "Admin Office"
                    };

                    var createResult = await userManager.CreateAsync(admin, adminPassword);
                    if (createResult.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
                        if (roleResult.Succeeded)
                        {
                            Console.WriteLine($"Admin user created successfully: {adminEmail}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to assign Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine("Admin user already exists");

                    if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
                    {
                        var roleResult = await userManager.AddToRoleAsync(existingAdmin, "Admin");
                        if (roleResult.Succeeded)
                        {
                            Console.WriteLine("Added Admin role to existing user");
                        }
                    }

                    if (!existingAdmin.IsAdmin)
                    {
                        existingAdmin.IsAdmin = true;
                        await userManager.UpdateAsync(existingAdmin);
                        Console.WriteLine("Updated IsAdmin flag for existing user");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating admin user: {ex.Message}");
            }
        }
    }
}