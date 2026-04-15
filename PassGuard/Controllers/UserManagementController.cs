using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.DAL;
using PassGuard.Models;
using PassGuard.Models.ViewModels;

namespace PassGuard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly HomeService _homeService;
        private readonly AuditLogService _auditLogService;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            HomeService homeService,
            AuditLogService auditLogService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _homeService = homeService;
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index()
        {
            List<UserListItemViewModel> users = new List<UserListItemViewModel>();
            List<ApplicationUser> applicationUsers = _userManager.Users
                .OrderBy(u => u.Email)
                .ToList();

            foreach (ApplicationUser user in applicationUsers)
            {
                IList<string> roles = await _userManager.GetRolesAsync(user);
                Home? home = _homeService.GetByOwnerUserId(user.Id);

                users.Add(new UserListItemViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    RoleName = roles.FirstOrDefault() ?? "Unassigned",
                    HomeAddress = home?.Address,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            return View(users);
        }

        public IActionResult Create()
        {
            return View(new UserCreateViewModel
            {
                AvailableRoles = GetAvailableRoles()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            model.FullName = model.FullName.Trim();
            model.Email = model.Email.Trim();

            if (!await _roleManager.RoleExistsAsync(model.RoleName))
            {
                ModelState.AddModelError(nameof(model.RoleName), "Select a valid role.");
            }

            ApplicationUser? existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            ApplicationUser user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                MustChangePassword = true
            };

            IdentityResult createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (IdentityError error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (!addToRoleResult.Succeeded)
            {
                foreach (IdentityError error in addToRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await _userManager.DeleteAsync(user);
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            _auditLogService.Log(
                "User Created",
                "ApplicationUser",
                user.Id,
                _userManager.GetUserId(User) ?? "",
                User.Identity?.Name ?? "",
                $"Created user {user.Email} with role {model.RoleName}.");

            TempData["SuccessMessage"] = $"Created {user.Email} and assigned the {model.RoleName} role.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditRole(string id)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            UserRoleEditViewModel model = new UserRoleEditViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "",
                AvailableRoles = GetAvailableRoles()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(UserRoleEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            ApplicationUser? user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            string? currentAdminUserId = _userManager.GetUserId(User);

            if (user.Id == currentAdminUserId && !string.Equals(model.RoleName, "Admin", StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "You cannot remove your own Admin role.");
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            IList<string> currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                IdentityResult removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Could not update the user's role.");
                    model.AvailableRoles = GetAvailableRoles();
                    return View(model);
                }
            }

            IdentityResult addResult = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (!addResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Could not assign the selected role.");
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            _auditLogService.Log(
                "Role Changed",
                "ApplicationUser",
                user.Id,
                _userManager.GetUserId(User) ?? "",
                User.Identity?.Name ?? "",
                $"Changed role for {user.Email} to {model.RoleName}.");

            return RedirectToAction(nameof(Index));
        }

        private List<string> GetAvailableRoles()
        {
            return _roleManager.Roles
                .Select(r => r.Name ?? "")
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .OrderBy(r => r)
                .ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "You cannot lock your own account.";
                return RedirectToAction(nameof(Index));
            }

            bool isLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;

            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                _auditLogService.Log(
                    "User Enabled",
                    "ApplicationUser",
                    user.Id,
                    _userManager.GetUserId(User) ?? "",
                    User.Identity?.Name ?? "",
                    $"Unlocked user {user.Email}.");
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                _auditLogService.Log(
                    "User Disabled",
                    "ApplicationUser",
                    user.Id,
                    _userManager.GetUserId(User) ?? "",
                    User.Identity?.Name ?? "",
                    $"Locked user {user.Email}.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
