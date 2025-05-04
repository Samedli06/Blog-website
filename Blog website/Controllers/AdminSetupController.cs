using Blog_website.Data;
using Blog_website.Models;
using Blog_website.Models.DTOs;
using Blog_website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog_website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSetupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminSetupController(AppDbContext context)
        {
            _context = context;
        }

        // This is a simplified endpoint for creating the first admin user
        // In production, you would want to secure this or remove it after initial setup
        [HttpPost("create-first-admin")]
        public async Task<IActionResult> CreateFirstAdmin([FromBody] CreateFirstAdminDto model)
        {
            // Check if there are any users with admin role already
            var adminRoleExists = await _context.Roles.AnyAsync(r => r.Name == "Admin");
            var adminRole = adminRoleExists 
                ? await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin")
                : new Role { Name = "Admin", Description = "Administrator with full access" };
            
            if (!adminRoleExists)
            {
                _context.Roles.Add(adminRole);
                await _context.SaveChangesAsync();
            }

            // Create the admin user
            var salt = PasswordHasher.GenerateSalt();
            var passwordHash = PasswordHasher.HashPassword(model.Password, salt);

            var adminUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHash,
                Salt = salt,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RegisteredDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            // Assign admin role
            _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await _context.SaveChangesAsync();

            // Create author role if needed
            var authorRoleExists = await _context.Roles.AnyAsync(r => r.Name == "Author");
            var authorRole = authorRoleExists
                ? await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Author")
                : new Role { Name = "Author", Description = "Can create and manage blog posts" };

            if (!authorRoleExists)
            {
                _context.Roles.Add(authorRole);
                await _context.SaveChangesAsync();
            }

            // Assign author role
            _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = authorRole.Id });
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Admin user created successfully", 
                credentials = new {
                    email = model.Email,
                    password = "[HIDDEN]" // Don't return the actual password
                }
            });
        }
    }

    public class CreateFirstAdminDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
