using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_website.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; }

        [MaxLength(200)]
        public string ProfilePictureUrl { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.Now;

        // Foreign key for User (one-to-one relationship)
        public int? UserId { get; set; }

        // Navigation property for Posts
        public ICollection<Post> Posts { get; set; }

        // Full name property
        [MaxLength(201)]
        public string FullName => $"{FirstName} {LastName}";
    }
}
