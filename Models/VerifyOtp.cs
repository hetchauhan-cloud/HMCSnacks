using System.ComponentModel.DataAnnotations;

namespace HMCSnacks.Models
{
    public class VerifyOtp
    {
        [Required]
        public string Otp { get; set; }
    }
}
