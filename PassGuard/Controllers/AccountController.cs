using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PassGuard.BLL;
using PassGuard.DAL;
using PassGuard.Models.ViewModels;

namespace PassGuard.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditLogService _auditLogService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            AuditLogService auditLogService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleHome();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            _auditLogService.Log(
                "User Login",
                "ApplicationUser",
                user.Id,
                user.Id,
                user.Email ?? "",
                $"Successful login for {user.Email}.");

            if (user.MustChangePassword)
            {
                TempData["InfoMessage"] = "You must change your temporary password before continuing.";
                return RedirectToAction(nameof(ChangePassword));
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToRoleHome();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new ChangePasswordViewModel
            {
                IsFirstLoginPasswordChange = user.MustChangePassword
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            model.IsFirstLoginPasswordChange = user.MustChangePassword;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool wasForcedPasswordChange = user.MustChangePassword;
            IdentityResult result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            _auditLogService.Log(
                "Password Changed",
                "ApplicationUser",
                user.Id,
                user.Id,
                user.Email ?? "",
                wasForcedPasswordChange
                    ? $"Completed forced password change for {user.Email}."
                    : $"Changed password for {user.Email}.");

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToRoleHome();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToRoleHome()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (User.IsInRole("HomeOwner"))
            {
                return RedirectToAction("Dashboard", "HomeOwner");
            }

            if (User.IsInRole("Security"))
            {
                return RedirectToAction("Panel", "Security");
            }

            return RedirectToAction("Index", "VisitPass");
        }
    }
}
