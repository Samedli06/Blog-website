using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog_website.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        // Navigation property
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
