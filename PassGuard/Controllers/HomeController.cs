using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.Models;
using PassGuard.Models.ViewModels;

namespace PassGuard.Controllers
{
    public class HomeController : Controller
    {
        private readonly HomeService _homeService;
        private readonly EstateService _estateService;
        private readonly VisitPassService _visitPassService;
        private readonly GateCheckInService _gateCheckInService;

        public HomeController(
            HomeService homeService,
            EstateService estateService,
            VisitPassService visitPassService,
            GateCheckInService gateCheckInService)
        {
            _homeService = homeService;
            _estateService = estateService;
            _visitPassService = visitPassService;
            _gateCheckInService = gateCheckInService;
        }

        public IActionResult Index()
        {
            var homes = _homeService.GetAllWithDetails();
            return View(homes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(PropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Estate? estate = _estateService.GetByName(model.EstateName);

            if (estate == null)
            {
                estate = new Estate
                {
                    EstateName = model.EstateName
                };
                _estateService.Add(estate);
            }

            Home home = new Home
            {
                OwnerName = model.OwnerName,
                Address = model.Address,
                EstateId = estate.EstateId
            };

            _homeService.Add(home);

            DateTime now = DateTime.Now;
            DateTime expire = now.AddDays(1);

            VisitPass visitPass = new VisitPass
            {
                VisitorName = model.VisitorName,
                VisitorPhone = model.VisitorPhone,
                HomeId = home.HomeId,
                CreatedAt = now,
                ExpiresAt = expire,
                Status = "Active"
            };

            _visitPassService.Add(visitPass);

            GateCheckIn gateCheckIn = new GateCheckIn
            {
                VisitPassId = visitPass.VisitPassId,
                Result = "Approved",
                CheckInTime = now,
                Note = "Auto Created"
            };

            _gateCheckInService.Add(gateCheckIn);

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Home? home = _homeService.GetFullDetails(id);

            if (home == null)
            {
                return NotFound();
            }

            VisitPass? visitPass = home.VisitPasses.FirstOrDefault();

            PropertyViewModel model = new PropertyViewModel
            {
                HomeId = home.HomeId,
                EstateName = home.Estate.EstateName,
                OwnerName = home.OwnerName,
                Address = home.Address,
                VisitorName = visitPass?.VisitorName ?? "",
                VisitorPhone = visitPass?.VisitorPhone ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(PropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Estate? estate = _estateService.GetByName(model.EstateName);

            if (estate == null)
            {
                estate = new Estate
                {
                    EstateName = model.EstateName
                };
                _estateService.Add(estate);
            }

            Home? home = _homeService.GetById(model.HomeId);

            if (home == null)
            {
                return NotFound();
            }

            home.OwnerName = model.OwnerName;
            home.Address = model.Address;
            home.EstateId = estate.EstateId;

            _homeService.Update(home);

            VisitPass? visitPass = _visitPassService.GetFirstByHomeId(model.HomeId);

            if (visitPass != null)
            {
                visitPass.VisitorName = model.VisitorName;
                visitPass.VisitorPhone = model.VisitorPhone;
                _visitPassService.Update(visitPass);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            Home? home = _homeService.GetFullDetails(id);

            if (home == null)
            {
                return NotFound();
            }

            return View(home);
        }

        public IActionResult Delete(int id)
        {
            _homeService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}