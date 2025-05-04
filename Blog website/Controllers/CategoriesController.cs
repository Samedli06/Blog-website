using Blog_website.Data;
using Blog_website.Models;
using Blog_website.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog_website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    PostCount = c.Posts.Count
                })
                .ToListAsync();
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                PostCount = category.Posts.Count
            };
        }

        // GET: api/Categories/slug/technology
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryBySlug(string slug)
        {
            var category = await _context.Categories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
            {
                return NotFound();
            }

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                PostCount = category.Posts.Count
            };
        }

        // GET: api/Categories/5/posts
        [HttpGet("{id}/posts")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetCategoryPosts(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var posts = await _context.Posts
                .Where(p => p.CategoryId == id && p.IsPublished)
                .Include(p => p.Author)
                .OrderByDescending(p => p.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Summary = p.Summary,
                    PublishedDate = p.PublishedDate,
                    FeaturedImageUrl = p.FeaturedImageUrl,
                    Slug = p.Slug,
                    ViewCount = p.ViewCount,
                    AuthorName = p.Author.FullName
                })
                .ToListAsync();

            return posts;
        }

        // POST: api/Categories
        [HttpPost]
        [Authorize(Roles = "Admin,Author")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            // Generate slug if not provided
            var slug = createCategoryDto.Slug;
            if (string.IsNullOrEmpty(slug))
            {
                slug = createCategoryDto.Name.ToLower().Replace(" ", "-");
                // Remove special characters
                slug = System.Text.RegularExpressions.Regex.Replace(slug, "[^a-z0-9\\-]", "");
            }

            // Check if slug already exists
            if (await _context.Categories.AnyAsync(c => c.Slug == slug))
            {
                return BadRequest(new { message = "A category with this slug already exists" });
            }

            var category = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description,
                Slug = slug
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                PostCount = 0
            });
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Update properties if provided
            if (!string.IsNullOrEmpty(updateCategoryDto.Name))
            {
                category.Name = updateCategoryDto.Name;
            }

            if (updateCategoryDto.Description != null)
            {
                category.Description = updateCategoryDto.Description;
            }

            // Update slug if provided
            if (!string.IsNullOrEmpty(updateCategoryDto.Slug) && updateCategoryDto.Slug != category.Slug)
            {
                // Check if slug already exists
                if (await _context.Categories.AnyAsync(c => c.Slug == updateCategoryDto.Slug && c.Id != id))
                {
                    return BadRequest(new { message = "A category with this slug already exists" });
                }
                category.Slug = updateCategoryDto.Slug;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if category has posts
            var hasPosts = await _context.Posts.AnyAsync(p => p.CategoryId == id);
            if (hasPosts)
            {
                return BadRequest(new { message = "Cannot delete category with associated posts" });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
