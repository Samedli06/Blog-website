using Blog_website.Data;
using Blog_website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blog_website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthorProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthorProfileController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/AuthorProfile
        [HttpGet]
        public async Task<ActionResult<AuthorProfileDto>> GetMyAuthorProfile()
        {
            // Get current user ID
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Check if user has an author profile
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.UserId == userId);
            if (author == null)
            {
                return NotFound(new { message = "You don't have an author profile yet" });
            }

            return new AuthorProfileDto
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                FullName = author.FullName,
                Email = author.Email,
                Bio = author.Bio,
                ProfilePictureUrl = author.ProfilePictureUrl,
                JoinedDate = author.JoinedDate
            };
        }

        // POST: api/AuthorProfile
        [HttpPost]
        public async Task<ActionResult<AuthorProfileDto>> CreateAuthorProfile(CreateAuthorProfileDto createDto)
        {
            // Get current user ID
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Check if user already has an author profile
            if (await _context.Authors.AnyAsync(a => a.UserId == userId))
            {
                return BadRequest(new { message = "You already have an author profile" });
            }

            // Get user details
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if user has Author role
            var hasAuthorRole = await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == "Author");

            // If not, add Author role
            if (!hasAuthorRole)
            {
                var authorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Author");
                if (authorRole == null)
                {
                    authorRole = new Role { Name = "Author", Description = "Can create and manage blog posts" };
                    _context.Roles.Add(authorRole);
                    await _context.SaveChangesAsync();
                }

                _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = authorRole.Id });
                await _context.SaveChangesAsync();
            }

            // Create author profile
            var author = new Author
            {
                FirstName = createDto.FirstName ?? user.FirstName,
                LastName = createDto.LastName ?? user.LastName,
                Email = user.Email,
                Bio = createDto.Bio,
                ProfilePictureUrl = createDto.ProfilePictureUrl,
                JoinedDate = DateTime.UtcNow,
                UserId = userId
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyAuthorProfile), new AuthorProfileDto
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                FullName = author.FullName,
                Email = author.Email,
                Bio = author.Bio,
                ProfilePictureUrl = author.ProfilePictureUrl,
                JoinedDate = author.JoinedDate
            });
        }

        // PUT: api/AuthorProfile
        [HttpPut]
        public async Task<IActionResult> UpdateAuthorProfile(UpdateAuthorProfileDto updateDto)
        {
            // Get current user ID
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Check if user has an author profile
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.UserId == userId);
            if (author == null)
            {
                return NotFound(new { message = "You don't have an author profile yet" });
            }

            // Update author profile
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                author.FirstName = updateDto.FirstName;

            if (!string.IsNullOrEmpty(updateDto.LastName))
                author.LastName = updateDto.LastName;

            if (!string.IsNullOrEmpty(updateDto.Bio))
                author.Bio = updateDto.Bio;

            if (!string.IsNullOrEmpty(updateDto.ProfilePictureUrl))
                author.ProfilePictureUrl = updateDto.ProfilePictureUrl;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class AuthorProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
        public DateTime JoinedDate { get; set; }
    }

    public class CreateAuthorProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
    }

    public class UpdateAuthorProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
