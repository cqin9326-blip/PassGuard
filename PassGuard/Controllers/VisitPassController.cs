using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.Models;
using PassGuard.Models.ViewModels;

namespace PassGuard.Controllers
{
    [Authorize]
    public class VisitPassController : Controller
    {
        private readonly VisitPassService _visitPassService;
        private readonly HomeService _homeService;
        private readonly EstateService _estateService;
        private readonly VisitorService _visitorService;
        private readonly GateCheckInService _gateCheckInService;

        public VisitPassController(
            VisitPassService visitPassService,
            HomeService homeService,
            EstateService estateService,
            VisitorService visitorService,
            GateCheckInService gateCheckInService)
        {
            _visitPassService = visitPassService;
            _homeService = homeService;
            _estateService = estateService;
            _visitorService = visitorService;
            _gateCheckInService = gateCheckInService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public IActionResult Index()
        {
            var visitPasses = _visitPassService.GetAllWithDetails();
            return View(visitPasses);
        }

        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Create(AccessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string ownerUserId = User.IsInRole("HomeOwner") ? CurrentUserId : model.OwnerUserId;

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
                    OwnerUserId = ownerUserId,
                    Address = model.Address,
                    EstateId = estate.EstateId
                };
                _homeService.Add(home);
            }
            else
            {
                home.OwnerUserId = ownerUserId;
                _homeService.Update(home);
            }

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
                CreatedByUserId = CurrentUserId,
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
                SecurityUserId = User.IsInRole("Security") ? CurrentUserId : "system"
            };

            _gateCheckInService.Add(gateCheckIn);

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Edit(int id)
        {
            VisitPass? visitPass = _visitPassService.GetFullDetails(id);

            if (visitPass == null)
            {
                return NotFound();
            }

            GateCheckIn? latestCheckIn = visitPass.GateCheckIn;

            AccessViewModel model = new AccessViewModel
            {
                VisitPassId = visitPass.VisitPassId,
                HomeId = visitPass.HomeId,
                GateCheckInId = latestCheckIn?.GateCheckInId,

                EstateName = visitPass.Home.Estate.EstateName,
                OwnerUserId = visitPass.Home.OwnerUserId,
                Address = visitPass.Home.Address,

                VisitorFullName = visitPass.Visitor.FullName,
                VisitorPhone = visitPass.Visitor.Phone,

                CodeHash = visitPass.CodeHash,
                CreatedByUserId = visitPass.CreatedByUserId,
                Status = visitPass.Status,
                CreatedAt = visitPass.CreatedAt,
                ExpiresAt = visitPass.ExpiresAt,

                SecurityUserId = latestCheckIn?.SecurityUserId ?? "",
                CheckInResult = latestCheckIn?.Result ?? "",
                CheckInTime = latestCheckIn?.CheckInTime,
                CheckInNote = latestCheckIn?.Note ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Edit(AccessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string ownerUserId = User.IsInRole("HomeOwner") ? CurrentUserId : model.OwnerUserId;

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

            home.OwnerUserId = ownerUserId;
            home.Address = model.Address;
            home.EstateId = estate.EstateId;
            _homeService.Update(home);

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

            visitPass.CodeHash = model.CodeHash;
            visitPass.CreatedByUserId = string.IsNullOrWhiteSpace(CurrentUserId) ? model.CreatedByUserId : CurrentUserId;
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
                    Note = model.CheckInNote,
                    SecurityUserId = User.IsInRole("Security") ? CurrentUserId : model.SecurityUserId
                };
                _gateCheckInService.Add(gateCheckIn);
            }
            else
            {
                gateCheckIn.Result = model.CheckInResult;
                gateCheckIn.CheckInTime = model.CheckInTime ?? DateTime.Now;
                gateCheckIn.Note = model.CheckInNote;
                gateCheckIn.SecurityUserId = User.IsInRole("Security") ? CurrentUserId : model.SecurityUserId;
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

        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Delete(int id)
        {
            _visitPassService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
