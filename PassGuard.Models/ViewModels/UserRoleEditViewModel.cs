using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models.ViewModels
{
    public class UserRoleEditViewModel
    {
        [Required]
        public string UserId { get; set; } = "";

        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";

        [Required]
        public string RoleName { get; set; } = "";

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
