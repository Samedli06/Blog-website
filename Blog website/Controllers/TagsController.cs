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
    public class TagsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TagsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
        {
            return await _context.Tags
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    PostCount = t.PostTags.Count
                })
                .ToListAsync();
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTag(int id)
        {
            var tag = await _context.Tags
                .Include(t => t.PostTags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                PostCount = tag.PostTags.Count
            };
        }

        // GET: api/Tags/slug/technology
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<TagDto>> GetTagBySlug(string slug)
        {
            var tag = await _context.Tags
                .Include(t => t.PostTags)
                .FirstOrDefaultAsync(t => t.Slug == slug);

            if (tag == null)
            {
                return NotFound();
            }

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                PostCount = tag.PostTags.Count
            };
        }

        // GET: api/Tags/5/posts
        [HttpGet("{id}/posts")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetTagPosts(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var tag = await _context.Tags.FindAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            var posts = await _context.PostTags
                .Where(pt => pt.TagId == id)
                .Include(pt => pt.Post)
                    .ThenInclude(p => p.Author)
                .Where(pt => pt.Post.IsPublished)
                .OrderByDescending(pt => pt.Post.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pt => new PostDto
                {
                    Id = pt.Post.Id,
                    Title = pt.Post.Title,
                    Summary = pt.Post.Summary,
                    PublishedDate = pt.Post.PublishedDate,
                    FeaturedImageUrl = pt.Post.FeaturedImageUrl,
                    Slug = pt.Post.Slug,
                    ViewCount = pt.Post.ViewCount,
                    AuthorName = pt.Post.Author.FullName
                })
                .ToListAsync();

            return posts;
        }

        // POST: api/Tags
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto)
        {
            // Generate slug if not provided
            var slug = createTagDto.Slug;
            if (string.IsNullOrEmpty(slug))
            {
                slug = createTagDto.Name.ToLower().Replace(" ", "-");
                // Remove special characters
                slug = System.Text.RegularExpressions.Regex.Replace(slug, "[^a-z0-9\\-]", "");
            }

            // Check if slug already exists
            if (await _context.Tags.AnyAsync(t => t.Slug == slug))
            {
                return BadRequest(new { message = "A tag with this slug already exists" });
            }

            var tag = new Tag
            {
                Name = createTagDto.Name,
                Slug = slug
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                PostCount = 0
            });
        }

        // PUT: api/Tags/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTag(int id, UpdateTagDto updateTagDto)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            // Update name if provided
            if (!string.IsNullOrEmpty(updateTagDto.Name))
            {
                tag.Name = updateTagDto.Name;
            }

            // Update slug if provided
            if (!string.IsNullOrEmpty(updateTagDto.Slug) && updateTagDto.Slug != tag.Slug)
            {
                // Check if slug already exists
                if (await _context.Tags.AnyAsync(t => t.Slug == updateTagDto.Slug && t.Id != id))
                {
                    return BadRequest(new { message = "A tag with this slug already exists" });
                }
                tag.Slug = updateTagDto.Slug;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            // Check if tag has posts
            var hasPosts = await _context.PostTags.AnyAsync(pt => pt.TagId == id);
            if (hasPosts)
            {
                return BadRequest(new { message = "Cannot delete tag with associated posts" });
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}
