using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.Models;
using PassGuard.Models.ViewModels;

namespace PassGuard.Controllers
{
    public class VisitPassController : Controller
    {
        private readonly VisitPassService _visitPassService;
        private readonly HomeService _homeService;
        private readonly EstateService _estateService;
        private readonly GateCheckInService _gateCheckInService;

        public VisitPassController(
            VisitPassService visitPassService,
            HomeService homeService,
            EstateService estateService,
            GateCheckInService gateCheckInService)
        {
            _visitPassService = visitPassService;
            _homeService = homeService;
            _estateService = estateService;
            _gateCheckInService = gateCheckInService;
        }

        public IActionResult Index()
        {
            var visitPasses = _visitPassService.GetAllWithDetails();
            return View(visitPasses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(AccessViewModel model)
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

            Home? home = _homeService.GetByAddressAndEstateId(model.Address, estate.EstateId);

            if (home == null)
            {
                home = new Home
                {
                    OwnerName = model.OwnerName,
                    Address = model.Address,
                    EstateId = estate.EstateId
                };
                _homeService.Add(home);
            }
            else
            {
                home.OwnerName = model.OwnerName;
                _homeService.Update(home);
            }

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
            VisitPass? visitPass = _visitPassService.GetFullDetails(id);

            if (visitPass == null)
            {
                return NotFound();
            }

            GateCheckIn? latestCheckIn = visitPass.GateCheckIns
                .OrderByDescending(g => g.CheckInTime)
                .FirstOrDefault();

            AccessViewModel model = new AccessViewModel
            {
                VisitPassId = visitPass.VisitPassId,
                HomeId = visitPass.HomeId,
                GateCheckInId = latestCheckIn?.GateCheckInId,

                EstateName = visitPass.Home.Estate.EstateName,
                OwnerName = visitPass.Home.OwnerName,
                Address = visitPass.Home.Address,

                VisitorName = visitPass.VisitorName,
                VisitorPhone = visitPass.VisitorPhone,

                Status = visitPass.Status,
                CreatedAt = visitPass.CreatedAt,
                ExpiresAt = visitPass.ExpiresAt,

                CheckInResult = latestCheckIn?.Result ?? "",
                CheckInTime = latestCheckIn?.CheckInTime,
                CheckInNote = latestCheckIn?.Note ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(AccessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            VisitPass? visitPass = _visitPassService.GetFullDetails(model.VisitPassId);

            if (visitPass == null)
            {
                return NotFound();
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

            Home? home = _homeService.GetById(visitPass.HomeId);

            if (home == null)
            {
                return NotFound();
            }

            home.OwnerName = model.OwnerName;
            home.Address = model.Address;
            home.EstateId = estate.EstateId;
            _homeService.Update(home);

            visitPass.VisitorName = model.VisitorName;
            visitPass.VisitorPhone = model.VisitorPhone;
            visitPass.Status = model.Status;
            visitPass.CreatedAt = model.CreatedAt;
            visitPass.ExpiresAt = model.ExpiresAt;
            _visitPassService.Update(visitPass);

            GateCheckIn? gateCheckIn;

            if (model.GateCheckInId.HasValue)
            {
                gateCheckIn = _gateCheckInService.GetById(model.GateCheckInId.Value);
            }
            else
            {
                gateCheckIn = null;
            }

            if (gateCheckIn == null)
            {
                gateCheckIn = new GateCheckIn
                {
                    VisitPassId = visitPass.VisitPassId,
                    Result = model.CheckInResult,
                    CheckInTime = model.CheckInTime ?? DateTime.Now,
                    Note = model.CheckInNote
                };
                _gateCheckInService.Add(gateCheckIn);
            }
            else
            {
                gateCheckIn.Result = model.CheckInResult;
                gateCheckIn.CheckInTime = model.CheckInTime ?? DateTime.Now;
                gateCheckIn.Note = model.CheckInNote;
                _gateCheckInService.Update(gateCheckIn);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            VisitPass? visitPass = _visitPassService.GetFullDetails(id);

            if (visitPass == null)
            {
                return NotFound();
            }

            return View(visitPass);
        }

        public IActionResult Delete(int id)
        {
            _visitPassService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}