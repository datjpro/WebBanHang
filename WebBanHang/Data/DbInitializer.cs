using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Create roles if they don't exist
            await CreateRolesAsync(roleManager);

            // Create admin user if it doesn't exist
            await CreateAdminUserAsync(userManager);
        }

        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { SD.Role_Admin, SD.Role_Employee, SD.Role_Company, SD.Role_Customer };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@datj.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "DATJ",
                    LastName = "Admin",
                    Address = "123 Admin Street",
                    PhoneNumber = "0123456789",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
                }
            }
            else
            {
                // Ensure admin user has admin role
                var isInRole = await userManager.IsInRoleAsync(adminUser, SD.Role_Admin);
                if (!isInRole)
                {
                    await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
                }
            }
        }
    }
}
