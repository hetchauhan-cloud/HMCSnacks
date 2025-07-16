using System.ComponentModel.DataAnnotations;

namespace HMCSnacks.Models
{
    public class ApplicationUser
    {
        [Key]
        public int Id { get; set; }

        // Basic Details
        [Required]
        public string Name { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User";

        [Phone]
        [StringLength(15)]
        public string MobileNumber { get; set; }

        // Location Info
        public int? StateId { get; set; }

        public int? CityId { get; set; }

        [StringLength(6, ErrorMessage = "Pincode must be 6 digits")]
        public string Pincode { get; set; }

        // Security
        public bool IsTwoFactorEnabled { get; set; } = true;

        [Required(ErrorMessage = "Address 1 is required")]
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? Address4 { get; set; }
    }
}
