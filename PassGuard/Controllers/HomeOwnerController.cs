using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;

namespace PassGuard.Controllers
{
    [Authorize(Roles = "HomeOwner")]
    public class HomeOwnerController : Controller
    {
        private readonly DashboardService _dashboardService;

        public HomeOwnerController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Dashboard()
        {
            string ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var vm = _dashboardService.GetHomeOwnerDashboard(ownerUserId);
            return View(vm);
        }
    }
}
