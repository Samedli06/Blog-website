using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog_website.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Salt { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(200)]
        public string? ProfilePictureUrl { get; set; } = null;

        // Default role will be handled through UserRoles collection
        // Navigation property for roles
        public ICollection<UserRole> UserRoles { get; set; }

        // Navigation property - if a user can be an author
        public Author Author { get; set; }
    }
}
