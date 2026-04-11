using Microsoft.AspNetCore.Identity;

namespace PassGuard.DAL
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = "";
    }
}
