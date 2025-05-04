using Blog_website.Data;
using Blog_website.Models;
using Blog_website.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog_website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .OrderByDescending(p => p.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Summary = p.Summary,
                    Content = p.Content,
                    PublishedDate = p.PublishedDate,
                    UpdatedDate = p.UpdatedDate,
                    FeaturedImageUrl = p.FeaturedImageUrl,
                    IsPublished = p.IsPublished,
                    Slug = p.Slug,
                    ViewCount = p.ViewCount,
                    AuthorName = p.Author.FullName,
                    CategoryName = p.Category != null ? p.Category.Name : null
                })
                .ToListAsync();

            return posts;
        }

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Increment view count
            post.ViewCount++;
            await _context.SaveChangesAsync();

            var postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Summary = post.Summary,
                Content = post.Content,
                PublishedDate = post.PublishedDate,
                UpdatedDate = post.UpdatedDate,
                FeaturedImageUrl = post.FeaturedImageUrl,
                IsPublished = post.IsPublished,
                Slug = post.Slug,
                ViewCount = post.ViewCount,
                AuthorName = post.Author.FullName,
                CategoryName = post.Category != null ? post.Category.Name : null,
                Tags = post.PostTags.Select(pt => pt.Tag.Name).ToList()
            };

            return postDto;
        }

        // GET: api/Posts/slug/my-post-slug
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<PostDto>> GetPostBySlug(string slug)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (post == null)
            {
                return NotFound();
            }

            // Increment view count
            post.ViewCount++;
            await _context.SaveChangesAsync();

            var postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Summary = post.Summary,
                Content = post.Content,
                PublishedDate = post.PublishedDate,
                UpdatedDate = post.UpdatedDate,
                FeaturedImageUrl = post.FeaturedImageUrl,
                IsPublished = post.IsPublished,
                Slug = post.Slug,
                ViewCount = post.ViewCount,
                AuthorName = post.Author.FullName,
                CategoryName = post.Category != null ? post.Category.Name : null,
                Tags = post.PostTags.Select(pt => pt.Tag.Name).ToList()
            };

            return postDto;
        }

        // POST: api/Posts
        [HttpPost]
        [Authorize(Roles = "Admin,Author")]
        public async Task<ActionResult<PostDto>> CreatePost(CreatePostDto createPostDto)
        {
            // Validate author
            var authorId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.UserId == authorId);
            
            if (author == null)
            {
                return BadRequest(new { message = "You must be an author to create posts" });
            }

            // Generate slug if not provided
            var slug = createPostDto.Slug;
            if (string.IsNullOrEmpty(slug))
            {
                slug = createPostDto.Title.ToLower().Replace(" ", "-");
                // Remove special characters
                slug = System.Text.RegularExpressions.Regex.Replace(slug, "[^a-z0-9\\-]", "");
            }

            // Check if slug already exists
            if (await _context.Posts.AnyAsync(p => p.Slug == slug))
            {
                return BadRequest(new { message = "A post with this slug already exists" });
            }

            // Create post
            var post = new Post
            {
                Title = createPostDto.Title,
                Content = createPostDto.Content,
                Summary = createPostDto.Summary,
                PublishedDate = DateTime.UtcNow,
                IsPublished = createPostDto.IsPublished,
                FeaturedImageUrl = createPostDto.FeaturedImageUrl,
                AuthorId = author.Id,
                CategoryId = createPostDto.CategoryId,
                Slug = slug
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Add tags if provided
            if (createPostDto.TagIds != null && createPostDto.TagIds.Any())
            {
                foreach (var tagId in createPostDto.TagIds)
                {
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        _context.PostTags.Add(new PostTag { PostId = post.Id, TagId = tagId });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Get category name if available
            string categoryName = null;
            if (post.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(post.CategoryId);
                categoryName = category?.Name;
            }

            // Return the created post
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Summary = post.Summary,
                Content = post.Content,
                PublishedDate = post.PublishedDate,
                IsPublished = post.IsPublished,
                FeaturedImageUrl = post.FeaturedImageUrl,
                Slug = post.Slug,
                AuthorName = author.FullName,
                CategoryName = categoryName
            });
        }

        // PUT: api/Posts/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Author")]
        public async Task<IActionResult> UpdatePost(int id, UpdatePostDto updatePostDto)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Check if user is the author or an admin
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.UserId == userId);
            var isAdmin = User.IsInRole("Admin");

            if (author == null || (post.AuthorId != author.Id && !isAdmin))
            {
                return Forbid();
            }

            // Update post properties
            post.Title = updatePostDto.Title ?? post.Title;
            post.Content = updatePostDto.Content ?? post.Content;
            post.Summary = updatePostDto.Summary ?? post.Summary;
            post.FeaturedImageUrl = updatePostDto.FeaturedImageUrl ?? post.FeaturedImageUrl;
            post.CategoryId = updatePostDto.CategoryId ?? post.CategoryId;
            post.IsPublished = updatePostDto.IsPublished ?? post.IsPublished;
            post.UpdatedDate = DateTime.UtcNow;

            // Update slug if provided
            if (!string.IsNullOrEmpty(updatePostDto.Slug) && updatePostDto.Slug != post.Slug)
            {
                // Check if slug already exists
                if (await _context.Posts.AnyAsync(p => p.Slug == updatePostDto.Slug && p.Id != id))
                {
                    return BadRequest(new { message = "A post with this slug already exists" });
                }
                post.Slug = updatePostDto.Slug;
            }

            // Update tags if provided
            if (updatePostDto.TagIds != null)
            {
                // Remove existing tags
                var existingPostTags = await _context.PostTags.Where(pt => pt.PostId == id).ToListAsync();
                _context.PostTags.RemoveRange(existingPostTags);

                // Add new tags
                foreach (var tagId in updatePostDto.TagIds)
                {
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        _context.PostTags.Add(new PostTag { PostId = post.Id, TagId = tagId });
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
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

        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Author")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Check if user is the author or an admin
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.UserId == userId);
            var isAdmin = User.IsInRole("Admin");

            if (author == null || (post.AuthorId != author.Id && !isAdmin))
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }

}
