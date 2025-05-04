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
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Comments/post/5
        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetPostComments(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }

            var comments = await _context.Comments
                .Where(c => c.PostId == postId && c.IsApproved && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    Name = c.Name,
                    Email = c.Email,
                    Replies = _context.Comments
                        .Where(reply => reply.ParentCommentId == c.Id && reply.IsApproved)
                        .OrderBy(reply => reply.CreatedDate)
                        .Select(reply => new CommentDto
                        {
                            Id = reply.Id,
                            Content = reply.Content,
                            CreatedDate = reply.CreatedDate,
                            UpdatedDate = reply.UpdatedDate,
                            Name = reply.Name,
                            Email = reply.Email
                        }).ToList()
                })
                .ToListAsync();

            return comments;
        }

        // POST: api/Comments
        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment(CreateCommentDto createCommentDto)
        {
            // Validate post exists
            var post = await _context.Posts.FindAsync(createCommentDto.PostId);
            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }

            // Validate parent comment if provided
            if (createCommentDto.ParentCommentId.HasValue)
            {
                var parentComment = await _context.Comments.FindAsync(createCommentDto.ParentCommentId);
                if (parentComment == null)
                {
                    return NotFound(new { message = "Parent comment not found" });
                }

                // Ensure parent comment belongs to the same post
                if (parentComment.PostId != createCommentDto.PostId)
                {
                    return BadRequest(new { message = "Parent comment does not belong to the specified post" });
                }
            }

            // Determine if comment should be auto-approved
            // For now, auto-approve if user is authenticated
            bool isApproved = User.Identity.IsAuthenticated;

            var comment = new Comment
            {
                Content = createCommentDto.Content,
                CreatedDate = DateTime.UtcNow,
                Name = createCommentDto.Name,
                Email = createCommentDto.Email,
                PostId = createCommentDto.PostId,
                ParentCommentId = createCommentDto.ParentCommentId,
                IsApproved = isApproved
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedDate = comment.CreatedDate,
                Name = comment.Name,
                Email = comment.Email,
                IsApproved = comment.IsApproved
            });
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedDate = comment.CreatedDate,
                UpdatedDate = comment.UpdatedDate,
                Name = comment.Name,
                Email = comment.Email,
                IsApproved = comment.IsApproved
            };
        }

        // PUT: api/Comments/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(int id, UpdateCommentDto updateCommentDto)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Only admins can update any comment
            // Regular users can only update their own comments (by email)
            bool isAdmin = User.IsInRole("Admin");
            string userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

            if (!isAdmin && comment.Email != userEmail)
            {
                return Forbid();
            }

            comment.Content = updateCommentDto.Content;
            comment.UpdatedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Only admins can delete any comment
            // Regular users can only delete their own comments (by email)
            bool isAdmin = User.IsInRole("Admin");
            string userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

            if (!isAdmin && comment.Email != userEmail)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Comments/5/approve
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            comment.IsApproved = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
