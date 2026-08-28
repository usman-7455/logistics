using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace logistics.Models
{

    [Index(nameof(Email), IsUnique = true)]
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
