using System.ComponentModel.DataAnnotations;

namespace HMCSnacks.Models
{
    public class Verify2FAOTP
    {
        [Required]
        [StringLength(6, ErrorMessage = "OTP must be 6 digits")]
        public string OTP { get; set; }
    }
}
