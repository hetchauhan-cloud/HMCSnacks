using System.ComponentModel.DataAnnotations;

namespace HMCSnacks.Models
{
    public class ResetPassword
    {
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
