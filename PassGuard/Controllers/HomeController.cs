using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.Models;
using PassGuard.Models.ViewModels;

namespace PassGuard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly HomeService _homeService;
        private readonly EstateService _estateService;
        private readonly VisitorService _visitorService;
        private readonly VisitPassService _visitPassService;
        private readonly GateCheckInService _gateCheckInService;

        public HomeController(
            HomeService homeService,
            EstateService estateService,
            VisitorService visitorService,
            VisitPassService visitPassService,
            GateCheckInService gateCheckInService)
        {
            _homeService = homeService;
            _estateService = estateService;
            _visitorService = visitorService;
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
                OwnerUserId = model.OwnerUserId,
                Address = model.Address,
                EstateId = estate.EstateId
            };

            _homeService.Add(home);

            Visitor? visitor = _visitorService.GetByFullNameAndPhone(model.VisitorFullName, model.VisitorPhone);

            if (visitor == null)
            {
                visitor = new Visitor
                {
                    FullName = model.VisitorFullName,
                    Phone = model.VisitorPhone
                };
                _visitorService.Add(visitor);
            }

            DateTime now = DateTime.Now;
            DateTime expire = now.AddDays(1);

            VisitPass visitPass = new VisitPass
            {
                VisitorId = visitor.VisitorId,
                CodeHash = Guid.NewGuid().ToString("N"),
                CreatedByUserId = model.OwnerUserId,
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
                Note = "Auto Created",
                SecurityUserId = "system"
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
                OwnerUserId = home.OwnerUserId,
                Address = home.Address,
                VisitorFullName = visitPass?.Visitor.FullName ?? "",
                VisitorPhone = visitPass?.Visitor.Phone ?? ""
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

            home.OwnerUserId = model.OwnerUserId;
            home.Address = model.Address;
            home.EstateId = estate.EstateId;

            _homeService.Update(home);

            VisitPass? visitPass = _visitPassService.GetFirstByHomeId(model.HomeId);

            if (visitPass != null)
            {
                Visitor? visitor = _visitorService.GetById(visitPass.VisitorId);

                if (visitor == null)
                {
                    visitor = new Visitor
                    {
                        FullName = model.VisitorFullName,
                        Phone = model.VisitorPhone
                    };
                    _visitorService.Add(visitor);
                    visitPass.VisitorId = visitor.VisitorId;
                }
                else
                {
                    visitor.FullName = model.VisitorFullName;
                    visitor.Phone = model.VisitorPhone;
                    _visitorService.Update(visitor);
                }

                visitPass.CreatedByUserId = model.OwnerUserId;
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
