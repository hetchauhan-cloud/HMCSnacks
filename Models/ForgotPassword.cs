using System.ComponentModel.DataAnnotations;

namespace HMCSnacks.Models
{
    public class ForgotPassword
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
