using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly AuditLogService _auditLogService;

        public VisitPassController(
            VisitPassService visitPassService,
            HomeService homeService,
            EstateService estateService,
            VisitorService visitorService,
            GateCheckInService gateCheckInService,
            AuditLogService auditLogService)
        {
            _visitPassService = visitPassService;
            _homeService = homeService;
            _estateService = estateService;
            _visitorService = visitorService;
            _gateCheckInService = gateCheckInService;
            _auditLogService = auditLogService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        private void PopulateVisitors(int? selectedVisitorId = null)
        {
            ViewBag.Visitors = _visitorService.GetAll()
                .Select(v => new SelectListItem
                {
                    Value = v.VisitorId.ToString(),
                    Text = $"{v.FullName} ({v.Phone})",
                    Selected = v.VisitorId == selectedVisitorId
                })
                .ToList();
        }

        public IActionResult Index()
        {
            List<VisitPass> visitPasses = User.IsInRole("HomeOwner")
                ? _visitPassService.GetByCreatedByUserId(CurrentUserId)
                : _visitPassService.GetAllWithDetails();

            return View(visitPasses);
        }

        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Create()
        {
            if (User.IsInRole("HomeOwner"))
            {
                Home? home = _homeService.GetByOwnerUserId(CurrentUserId);

                if (home == null)
                {
                    TempData["ErrorMessage"] = "No home is assigned to your account yet. Please contact an admin.";
                    return RedirectToAction(nameof(Index));
                }

                PopulateVisitors();
                return View(new AccessViewModel
                {
                    EstateName = home.Estate.EstateName,
                    OwnerUserId = home.OwnerUserId,
                    Address = home.Address
                });
            }

            PopulateVisitors();
            return View(new AccessViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Create(AccessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            string ownerUserId = User.IsInRole("HomeOwner") ? CurrentUserId : model.OwnerUserId;
            Home? home;

            if (User.IsInRole("HomeOwner"))
            {
                home = _homeService.GetByOwnerUserId(CurrentUserId);

                if (home == null)
                {
                    ModelState.AddModelError(string.Empty, "No home is assigned to your account yet.");
                    return View(model);
                }

                model.OwnerUserId = home.OwnerUserId;
                model.Address = home.Address;
                model.EstateName = home.Estate.EstateName;
            }
            else
            {
                Estate? estate = _estateService.GetByName(model.EstateName);

                if (estate == null)
                {
                    estate = new Estate
                    {
                        EstateName = model.EstateName
                    };
                    _estateService.Add(estate);
                }

                home = _homeService.GetByAddressAndEstateId(model.Address, estate.EstateId);

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
            }

            Visitor? visitor = _visitorService.GetById(model.VisitorId);

            if (visitor == null)
            {
                ModelState.AddModelError(nameof(model.VisitorId), "Select a valid visitor.");
                PopulateVisitors();
                return View(model);
            }

            DateTime now = DateTime.Now;
            DateTime expire = now.AddDays(1);
            string plainCode = _visitPassService.GeneratePlainCode();

            VisitPass visitPass = new VisitPass
            {
                VisitorId = visitor.VisitorId,
                CodeHash = _visitPassService.HashCode(plainCode),
                CreatedByUserId = CurrentUserId,
                HomeId = home.HomeId,
                CreatedAt = now,
                ExpiresAt = expire,
                Status = PassStatuses.Active
            };

            _visitPassService.Add(visitPass);
            _auditLogService.Log(
                "Pass Created",
                "VisitPass",
                visitPass.VisitPassId.ToString(),
                CurrentUserId,
                User.Identity?.Name ?? "",
                $"Created pass for visitor {visitor.FullName} at {home.Address}.");

            TempData["NewPassCode"] = plainCode;
            TempData["SuccessMessage"] = "Visit pass created successfully. Save the code now because it will not be shown again.";

            return RedirectToAction("Details", new { id = visitPass.VisitPassId });
        }

        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Edit(int id)
        {
            VisitPass? visitPass = _visitPassService.GetFullDetails(id);

            if (visitPass == null)
            {
                return NotFound();
            }

            if (User.IsInRole("HomeOwner") && visitPass.CreatedByUserId != CurrentUserId)
            {
                return Forbid();
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

                VisitorId = visitPass.VisitorId,

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

            PopulateVisitors(model.VisitorId);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Edit(AccessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            string ownerUserId = User.IsInRole("HomeOwner") ? CurrentUserId : model.OwnerUserId;

            VisitPass? visitPass = _visitPassService.GetFullDetails(model.VisitPassId);

            if (visitPass == null)
            {
                return NotFound();
            }

            if (User.IsInRole("HomeOwner") && visitPass.CreatedByUserId != CurrentUserId)
            {
                return Forbid();
            }

            Home? home = _homeService.GetById(visitPass.HomeId);

            if (home == null)
            {
                return NotFound();
            }

            if (User.IsInRole("HomeOwner"))
            {
                if (home.OwnerUserId != CurrentUserId)
                {
                    return Forbid();
                }

                model.OwnerUserId = home.OwnerUserId;
                model.Address = home.Address;
                model.EstateName = home.Estate?.EstateName ?? model.EstateName;
            }
            else
            {
                Estate? estate = _estateService.GetByName(model.EstateName);

                if (estate == null)
                {
                    estate = new Estate
                    {
                        EstateName = model.EstateName
                    };
                    _estateService.Add(estate);
                }

                home.OwnerUserId = ownerUserId;
                home.Address = model.Address;
                home.EstateId = estate.EstateId;
                _homeService.Update(home);
            }

            Visitor? visitor = _visitorService.GetById(model.VisitorId);

            if (visitor == null)
            {
                ModelState.AddModelError(nameof(model.VisitorId), "Select a valid visitor.");
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            visitPass.VisitorId = visitor.VisitorId;
            visitPass.CodeHash = model.CodeHash;
            visitPass.CreatedByUserId = string.IsNullOrWhiteSpace(CurrentUserId) ? model.CreatedByUserId : CurrentUserId;
            if (string.Equals(model.Status, PassStatuses.Revoked, StringComparison.Ordinal))
            {
                _visitPassService.Revoke(visitPass);
            }
            visitPass.CreatedAt = model.CreatedAt;
            visitPass.ExpiresAt = model.ExpiresAt;
            _visitPassService.Update(visitPass);
            _visitPassService.NormalizeStatus(visitPass);

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

            if (User.IsInRole("HomeOwner") && visitPass.CreatedByUserId != CurrentUserId)
            {
                return Forbid();
            }

            return View(visitPass);
        }

        [Authorize(Roles = "Admin,Security")]
        public IActionResult Verify()
        {
            return View(new VerifyPassViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Security")]
        [ValidateAntiForgeryToken]
        public IActionResult Verify(VerifyPassViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.EnteredCode))
            {
                model.Message = "Enter a pass code to verify.";
                return View(model);
            }

            string enteredCode = model.EnteredCode.Trim().ToUpperInvariant();
            List<VisitPass> visitPasses = _visitPassService.GetAllWithDetails();
            VisitPass? matchedPass = visitPasses.FirstOrDefault(v => _visitPassService.VerifyCode(v, enteredCode));

            if (matchedPass == null)
            {
                model.Message = "Invalid pass code. No matching visit pass was found.";
                model.IsMatch = false;
                return View(model);
            }

            string currentStatus = _visitPassService.NormalizeStatus(matchedPass);

            if (_visitPassService.CanBeAccepted(matchedPass))
            {
                GateCheckIn? gateCheckIn = matchedPass.GateCheckIn;

                if (gateCheckIn == null)
                {
                    gateCheckIn = new GateCheckIn
                    {
                        VisitPassId = matchedPass.VisitPassId,
                        Result = "Approved",
                        CheckInTime = DateTime.Now,
                        Note = "Approved by pass-code verification.",
                        SecurityUserId = CurrentUserId
                    };
                    _gateCheckInService.Add(gateCheckIn);
                }
                else
                {
                    gateCheckIn.Result = "Approved";
                    gateCheckIn.CheckInTime = DateTime.Now;
                    gateCheckIn.Note = "Approved by pass-code verification.";
                    gateCheckIn.SecurityUserId = CurrentUserId;
                    _gateCheckInService.Update(gateCheckIn);
                }

                matchedPass.GateCheckIn = gateCheckIn;
                matchedPass.Status = PassStatuses.Used;
                _visitPassService.Update(matchedPass);

                model.Message = "Pass verified successfully. Visitor may enter.";
                model.IsMatch = true;
                model.VisitPass = _visitPassService.GetFullDetails(matchedPass.VisitPassId);
                _auditLogService.Log(
                    "Pass Verification",
                    "VisitPass",
                    matchedPass.VisitPassId.ToString(),
                    CurrentUserId,
                    User.Identity?.Name ?? "",
                    $"Approved code verification for visitor {matchedPass.Visitor.FullName}.");
                return View(model);
            }

            if (matchedPass.GateCheckIn == null)
            {
                GateCheckIn deniedCheckIn = new GateCheckIn
                {
                    VisitPassId = matchedPass.VisitPassId,
                    Result = "Denied",
                    CheckInTime = DateTime.Now,
                    Note = $"Verification denied because pass status is {currentStatus}.",
                    SecurityUserId = CurrentUserId
                };
                _gateCheckInService.Add(deniedCheckIn);
                matchedPass.GateCheckIn = deniedCheckIn;
            }

            model.Message = $"Pass found, but it is {currentStatus} and cannot be used.";
            model.IsMatch = false;
            model.VisitPass = _visitPassService.GetFullDetails(matchedPass.VisitPassId);
            _auditLogService.Log(
                "Pass Verification",
                "VisitPass",
                matchedPass.VisitPassId.ToString(),
                CurrentUserId,
                User.Identity?.Name ?? "",
                $"Denied code verification because pass status is {currentStatus}.");
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HomeOwner")]
        [ValidateAntiForgeryToken]
        public IActionResult Revoke(int id)
        {
            VisitPass? visitPass = _visitPassService.GetFullDetails(id);

            if (visitPass == null)
            {
                return NotFound();
            }

            if (User.IsInRole("HomeOwner") && visitPass.CreatedByUserId != CurrentUserId)
            {
                return Forbid();
            }

            _visitPassService.Revoke(visitPass);
            _auditLogService.Log(
                "Pass Revoked",
                "VisitPass",
                visitPass.VisitPassId.ToString(),
                CurrentUserId,
                User.Identity?.Name ?? "",
                $"Revoked pass for visitor {visitPass.Visitor.FullName}.");
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Delete(int id)
        {
            VisitPass? visitPass = _visitPassService.GetFullDetails(id);

            if (visitPass == null)
            {
                return NotFound();
            }

            if (User.IsInRole("HomeOwner") && visitPass.CreatedByUserId != CurrentUserId)
            {
                return Forbid();
            }

            _visitPassService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
