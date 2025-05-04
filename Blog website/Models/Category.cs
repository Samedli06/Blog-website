using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog_website.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string Slug { get; set; }

        // Navigation property
        public ICollection<Post> Posts { get; set; }
    }
}
