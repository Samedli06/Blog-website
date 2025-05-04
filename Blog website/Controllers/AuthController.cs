using Blog_website.Data;
using Blog_website.Models;
using Blog_website.Models.DTOs;
using Blog_website.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Blog_website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // Admin registration endpoint (secured with a secret key)
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            // Verify admin secret key (this should be stored in configuration)
            string adminSecretKey = "admin-secret-key-change-this-in-production";  // In production, use configuration
            if (model.AdminSecretKey != adminSecretKey)
                return Unauthorized(new { message = "Invalid admin secret key" });
                
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == model.Email || u.Username == model.Username))
                return BadRequest(new { message = "Username or email already exists" });
                
            // Generate salt and hash password
            var salt = PasswordHasher.GenerateSalt();
            var passwordHash = PasswordHasher.HashPassword(model.Password, salt);
                
            // Create new admin user
            var adminUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHash,
                Salt = salt,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RegisteredDate = DateTime.UtcNow,
                UserRoles = new List<UserRole>()
            };
                
            // Ensure Admin role exists
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                // Create the admin role if it doesn't exist
                adminRole = new Role { Name = "Admin", Description = "Administrator with full access" };
                _context.Roles.Add(adminRole);
                await _context.SaveChangesAsync();
            }
                
            // First add the user to the database
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
            
            // Now assign Admin role (after user has an ID)
            _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await _context.SaveChangesAsync();
                
            // Also add Author role if requested
            if (model.IsAuthor)
            {
                var authorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Author");
                if (authorRole == null)
                {
                    // Create the author role if it doesn't exist
                    authorRole = new Role { Name = "Author", Description = "Can create and manage blog posts" };
                    _context.Roles.Add(authorRole);
                    await _context.SaveChangesAsync();
                }
                
                // Add the author role
                _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = authorRole.Id });
                await _context.SaveChangesAsync();
                
                // Create author profile if requested
                if (model.CreateAuthorProfile)
                {
                    var author = new Author
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Bio = model.Bio,
                        ProfilePictureUrl = model.ProfilePictureUrl,
                        JoinedDate = DateTime.UtcNow,
                        UserId = adminUser.Id  // Set the user ID directly
                    };
                    
                    _context.Authors.Add(author);
                    await _context.SaveChangesAsync();
                }
            }
                
            // Generate and return token
            var token = _tokenService.GenerateJwtToken(adminUser);
            
            // Set token in HTTP-only cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(300)
            };
            
            Response.Cookies.Append("auth_token", token, cookieOptions);
            
            return Ok(new { message = "Admin user created successfully" });
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == model.Email || u.Username == model.Username))
                return BadRequest(new { message = "Username or email already exists" });

            // Generate salt and hash password
            var salt = PasswordHasher.GenerateSalt();
            var passwordHash = PasswordHasher.HashPassword(model.Password, salt);

            // Create new user
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHash,
                Salt = salt,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RegisteredDate = DateTime.UtcNow,
                UserRoles = new List<UserRole>()
            };

            // Assign default subscriber role
            var subscriberRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Subscriber");
            if (subscriberRole == null)
            {
                // Create the role if it doesn't exist
                subscriberRole = new Role { Name = "Subscriber", Description = "Regular user with basic privileges" };
                _context.Roles.Add(subscriberRole);
                await _context.SaveChangesAsync();
            }

            user.UserRoles.Add(new UserRole { User = user, Role = subscriberRole });

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Return token
            var token = _tokenService.GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find user by email and include roles
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            // Verify password
            if (!PasswordHasher.VerifyPassword(model.Password, user.Salt, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _tokenService.GenerateJwtToken(user);
            
            // Set token in HTTP-only cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Use in production with HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(300) // Match token expiration
            };
            
            Response.Cookies.Append("auth_token", token, cookieOptions);
            
            // Return success message without exposing the token
            return Ok(new { message = "Login successful" });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Get user ID from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                return Unauthorized();

            // Get user from database with roles included
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
                
            if (user == null)
                return NotFound();

            // Return user info (excluding sensitive data)
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                user.ProfilePictureUrl,
                user.RegisteredDate,
                user.LastLoginDate
            });
        }
    }

    // DTOs
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
