using Microsoft.AspNetCore.Identity;
using PassGuard.DAL;

namespace PassGuard.Infrastructure
{
    public static class IdentitySeedData
    {
        private static readonly (string Role, string Email, string FullName, string Password)[] DefaultUsers =
        {
            ("Admin", "admin@email.com", "PassGuard Admin", "Admin123!"),
            ("HomeOwner", "homeowner@email.com", "Demo HomeOwner", "HomeOwner123!"),
            ("Security", "security@email.com", "Demo Security", "Security123!")
        };

        public static async Task SeedAsync(IServiceProvider services)
        {
            using IServiceScope scope = services.CreateScope();

            RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (string role in new[] { "Admin", "HomeOwner", "Security" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            foreach ((string role, string email, string fullName, string password) in DefaultUsers)
            {
                ApplicationUser? user = await FindSeedUserAsync(userManager, role, email, fullName);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        EmailConfirmed = true,
                        MustChangePassword = role != "Admin"
                    };

                    IdentityResult createResult = await userManager.CreateAsync(user, password);

                    if (!createResult.Succeeded)
                    {
                        string errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to seed user '{email}': {errors}");
                    }
                }

                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }

                bool shouldForcePasswordChange = role != "Admin";
                bool requiresUpdate =
                    !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(user.UserName, email, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(user.FullName, fullName, StringComparison.Ordinal) ||
                    user.MustChangePassword != shouldForcePasswordChange ||
                    !user.EmailConfirmed;

                if (requiresUpdate)
                {
                    user.Email = email;
                    user.UserName = email;
                    user.FullName = fullName;
                    user.EmailConfirmed = true;
                    user.MustChangePassword = shouldForcePasswordChange;

                    IdentityResult updateResult = await userManager.UpdateAsync(user);

                    if (!updateResult.Succeeded)
                    {
                        string errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to update seeded user '{email}': {errors}");
                    }
                }
            }
        }

        private static async Task<ApplicationUser?> FindSeedUserAsync(
            UserManager<ApplicationUser> userManager,
            string role,
            string email,
            string fullName)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                return user;
            }

            IList<ApplicationUser> usersInRole = await userManager.GetUsersInRoleAsync(role);

            user = usersInRole.FirstOrDefault(u =>
                string.Equals(u.FullName, fullName, StringComparison.Ordinal));

            if (user != null)
            {
                return user;
            }

            return usersInRole.FirstOrDefault(u =>
                !string.IsNullOrWhiteSpace(u.Email) &&
                u.Email.EndsWith("@passguard.local", StringComparison.OrdinalIgnoreCase));
        }
    }
}
