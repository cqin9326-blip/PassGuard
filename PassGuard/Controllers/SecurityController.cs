using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;

namespace PassGuard.Controllers
{
    [Authorize(Roles = "Admin,Security")]
    public class SecurityController : Controller
    {
        private readonly DashboardService _dashboardService;

        public SecurityController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Panel()
        {
            var vm = _dashboardService.GetSecurityPanel();
            return View(vm);
        }
    }
}
