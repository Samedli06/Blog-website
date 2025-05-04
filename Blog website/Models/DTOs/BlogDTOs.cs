using System;
using System.Collections.Generic;

namespace Blog_website.Models.DTOs
{
    // Auth DTOs
    public class RegisterAdminDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AdminSecretKey { get; set; }
        public bool IsAuthor { get; set; } = true;
        public bool CreateAuthorProfile { get; set; } = true;
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
    }

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

    // Post DTOs
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string FeaturedImageUrl { get; set; }
        public bool IsPublished { get; set; }
        public string Slug { get; set; }
        public int ViewCount { get; set; }
        public string AuthorName { get; set; }
        public string CategoryName { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class PostSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime PublishedDate { get; set; }
        public string FeaturedImageUrl { get; set; }
        public string Slug { get; set; }
        public int ViewCount { get; set; }
        public string AuthorName { get; set; }
    }

    public class CreatePostDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string FeaturedImageUrl { get; set; }
        public bool IsPublished { get; set; } = true;
        public string Slug { get; set; }
        public int? CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();
    }

    public class UpdatePostDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string FeaturedImageUrl { get; set; }
        public bool? IsPublished { get; set; }
        public string Slug { get; set; }
        public int? CategoryId { get; set; }
        public List<int> TagIds { get; set; }
    }

    // Category DTOs
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public int PostCount { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
    }

    // Tag DTOs
    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int PostCount { get; set; }
    }

    public class CreateTagDto
    {
        public string Name { get; set; }
        public string Slug { get; set; }
    }

    public class UpdateTagDto
    {
        public string Name { get; set; }
        public string Slug { get; set; }
    }

    // Comment DTOs
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsApproved { get; set; }
        public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
    }

    public class CreateCommentDto
    {
        public string Content { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
    }

    public class UpdateCommentDto
    {
        public string Content { get; set; }
    }
}
