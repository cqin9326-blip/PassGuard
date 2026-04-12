using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.DAL;

namespace PassGuard.Controllers
{
    [Authorize(Roles = "HomeOwner")]
    public class HomeOwnerController : Controller
    {
        private readonly DashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeOwnerController(
            DashboardService dashboardService,
            UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            string ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var vm = _dashboardService.GetHomeOwnerDashboard(ownerUserId);
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            vm.HomeOwnerName = user?.FullName ?? User.Identity?.Name ?? "Resident";
            return View(vm);
        }
    }
}
