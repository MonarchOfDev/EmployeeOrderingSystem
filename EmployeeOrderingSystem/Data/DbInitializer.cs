using EmployeeOrderingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeOrderingSystem.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // 1. Seed Roles
            string[] roles = new[] { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2. Seed Users + Employees (only if no users exist)
            if (!userManager.Users.Any())
            {
                // --- Admin user ---
                var adminUser = new IdentityUser
                {
                    UserName = "admin@cafeteria.co.za",
                    Email = "admin@cafeteria.co.za",
                    EmailConfirmed = true
                };
                var adminResult = await userManager.CreateAsync(adminUser, "Admin@123!");
                if (adminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    dbContext.Employees.Add(new Employee
                    {
                        Name = "Staff Admin",
                        EmployeeNumber = "E001",
                        Balance = 0,
                        LastDepositMonth = DateTime.Now,
                        UserId = adminUser.Id
                    });
                }

                // --- Customer user ---
                var testUser = new IdentityUser
                {
                    UserName = "customer@cafeteria.co.za",
                    Email = "customer@cafeteria.co.za",
                    EmailConfirmed = true
                };
                var testResult = await userManager.CreateAsync(testUser, "Customer@123!");
                if (testResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, "Customer");
                    dbContext.Employees.Add(new Employee
                    {
                        Name = "Test Customer",
                        EmployeeNumber = "E002",
                        Balance = 500,
                        LastDepositMonth = DateTime.Now,
                        UserId = testUser.Id
                    });
                }

                await dbContext.SaveChangesAsync();
            }

            // 3. Seed Restaurants + MenuItems
            if (!dbContext.Restaurants.Any())
            {
                var restaurants = new List<Restaurant>
                {
                    new Restaurant
                    {
                        Name = "McDonald's Braamfontein",
                        LocationDescription = "Jan Smuts Avenue",
                        ContactNumber = "011-123-4567",
                        MenuItems = new List<MenuItem>
                        {
                            new MenuItem { Name = "Big Mac Meal", Description = "Big Mac burger, fries, drink", Price = 69.99m },
                            new MenuItem { Name = "Chicken McNuggets (6 pc)", Description = "6 pieces, with sauce", Price = 39.99m }
                        }
                    },
                    new Restaurant
                    {
                        Name = "KFC Braamfontein",
                        LocationDescription = "De Korte Street",
                        ContactNumber = "011-987-6543",
                        MenuItems = new List<MenuItem>
                        {
                            new MenuItem { Name = "Zinger Burger Meal", Description = "Zinger burger, fries, drink", Price = 64.99m },
                            new MenuItem { Name = "Streetwise Two", Description = "2 pieces chicken, small chips", Price = 34.99m }
                        }
                    },
                    new Restaurant
                    {
                        Name = "Steers Braamfontein",
                        LocationDescription = "Juta Street",
                        ContactNumber = "011-246-1357",
                        MenuItems = new List<MenuItem>
                        {
                            new MenuItem { Name = "Wacky Wednesday", Description = "2 burgers, 1 chips", Price = 49.99m },
                            new MenuItem { Name = "Rib King Burger Meal", Description = "Rib burger, fries, drink", Price = 74.99m }
                        }
                    }
                };

                dbContext.Restaurants.AddRange(restaurants);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
