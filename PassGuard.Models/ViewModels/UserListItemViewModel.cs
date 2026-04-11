namespace PassGuard.Models.ViewModels
{
    public class UserListItemViewModel
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string? HomeAddress { get; set; }
    }
}
