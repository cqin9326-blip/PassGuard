using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;

namespace PassGuard.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            var vm = _dashboardService.GetDashboard();
            return View(vm);
        }
    }
}