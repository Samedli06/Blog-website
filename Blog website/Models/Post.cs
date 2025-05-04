using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_website.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [MaxLength(500)]
        public string Summary { get; set; }

        [Required]
        public DateTime PublishedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [MaxLength(200)]
        public string FeaturedImageUrl { get; set; }

        public bool IsPublished { get; set; } = true;

        [Required]
        public int AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public Author Author { get; set; }

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public ICollection<PostTag> PostTags { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; }

        public int ViewCount { get; set; } = 0;
    }
}
