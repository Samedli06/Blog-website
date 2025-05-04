using System;
using System.ComponentModel.DataAnnotations;

namespace Blog_website.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime SubscribedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Confirmation token for double opt-in
        [MaxLength(100)]
        public string ConfirmationToken { get; set; }

        public bool IsConfirmed { get; set; } = false;

        public DateTime? ConfirmedDate { get; set; }

        // Tracking for unsubscribe
        public DateTime? UnsubscribedDate { get; set; }
    }
}
