using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog_website.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Slug { get; set; }

        // Navigation property
        public ICollection<PostTag> PostTags { get; set; }
    }
}
