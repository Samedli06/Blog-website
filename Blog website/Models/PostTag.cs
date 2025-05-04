using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_website.Models
{
    public class PostTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int TagId { get; set; }

        // Navigation properties
        [ForeignKey("PostId")]
        public Post Post { get; set; }

        [ForeignKey("TagId")]
        public Tag Tag { get; set; }
    }
}
