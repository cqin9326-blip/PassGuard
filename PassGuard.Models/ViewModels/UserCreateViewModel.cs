using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models.ViewModels
{
    public class UserCreateViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; } = "";

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
