using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMCSnacks.Models
{
    public class UserOTP
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(6)]
        public string OTP { get; set; }

        [Required]
        public DateTime ExpiryTime { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }  // ✅ Updated navigation
    }
}
