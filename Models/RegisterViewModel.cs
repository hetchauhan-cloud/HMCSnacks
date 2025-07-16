using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HMCSnacks.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Mobile Number")]
        [Phone]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be 10 digits.")]
        public string MobileNumber { get; set; }

        [Required]
        [Display(Name = "State")]
        public int? StateId { get; set; }

        [Required]
        [Display(Name = "City")]
        public int? CityId { get; set; }

        [Required]
        [Display(Name = "Pincode")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Pincode must be 6 digits.")]
        public string Pincode { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

        [Display(Name = "Address 3")]
        public string Address3 { get; set; }

        [Display(Name = "Address 4")]
        public string Address4 { get; set; }
    }
}
