using Blog_website.Models;
using Blog_website.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog_website.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(AppDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if roles exist, if not create them
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role { Name = "Admin", Description = "Administrator with full access" },
                    new Role { Name = "Author", Description = "Can create and manage blog posts" },
                    new Role { Name = "Editor", Description = "Can edit and approve content" },
                    new Role { Name = "Subscriber", Description = "Regular user with basic privileges" }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Check if admin user exists, if not create one
            // Use both username and email to check to avoid duplicates
            if (!await context.Users.AnyAsync(u => u.Email == "admin@blog.com" || u.Username == "admin"))
            {
                // Generate a simple salt and hash the password directly
                var salt = PasswordHasher.GenerateSalt();
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!" + salt);
                
                Console.WriteLine("Creating admin user with credentials:");
                Console.WriteLine("Email: admin@blog.com");
                Console.WriteLine("Password: Admin123!");

                // Create admin user
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@blog.com",
                    PasswordHash = passwordHash,
                    Salt = salt,
                    FirstName = "Admin",
                    LastName = "User",
                    RegisteredDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                // Get admin and author roles
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                var authorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Author");

                // Assign roles to admin user
                if (adminRole != null)
                {
                    await context.UserRoles.AddAsync(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
                }

                if (authorRole != null)
                {
                    await context.UserRoles.AddAsync(new UserRole { UserId = adminUser.Id, RoleId = authorRole.Id });
                }

                // Create author profile for admin
                var author = new Author
                {
                    FirstName = adminUser.FirstName,
                    LastName = adminUser.LastName,
                    Email = adminUser.Email,
                    Bio = "Blog Administrator",
                    JoinedDate = DateTime.UtcNow,
                    UserId = adminUser.Id
                };

                await context.Authors.AddAsync(author);
                await context.SaveChangesAsync();
            }
        }
    }
}
