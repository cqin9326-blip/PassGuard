using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PassGuard.BLL;
using PassGuard.DAL;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public VisitPassController(
            VisitPassService visitPassService,
            HomeService homeService,
            EstateService estateService,
            VisitorService visitorService,
            GateCheckInService gateCheckInService,
            AuditLogService auditLogService,
            UserManager<ApplicationUser> userManager)
        {
            _visitPassService = visitPassService;
            _homeService = homeService;
            _estateService = estateService;
            _visitorService = visitorService;
            _gateCheckInService = gateCheckInService;
            _auditLogService = auditLogService;
            _userManager = userManager;
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

        private async Task PopulateHomeOwnerUsersAsync(string? selectedUserId = null)
        {
            List<ApplicationUser> users = _userManager.Users.OrderBy(u => u.Email).ToList();
            List<SelectListItem> items = new List<SelectListItem>();

            foreach (ApplicationUser user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "HomeOwner"))
                {
                    items.Add(new SelectListItem
                    {
                        Value = user.Id,
                        Text = $"{user.FullName} ({user.Email})",
                        Selected = user.Id == selectedUserId
                    });
                }
            }

            ViewBag.HomeOwnerUsers = items;
        }

        private async Task<bool> IsValidHomeOwnerUserAsync(string ownerUserId)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(ownerUserId);
            return user != null && await _userManager.IsInRoleAsync(user, "HomeOwner");
        }

        private async Task PopulateSelectedHomeOwnerNameAsync(string? ownerUserId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                ViewBag.SelectedHomeOwnerName = "Assigned homeowner";
                return;
            }

            ApplicationUser? user = await _userManager.FindByIdAsync(ownerUserId);
            ViewBag.SelectedHomeOwnerName = user?.FullName ?? user?.Email ?? ownerUserId;
        }

        public IActionResult Index()
        {
            List<VisitPass> visitPasses = User.IsInRole("HomeOwner")
                ? _visitPassService.GetByCreatedByUserId(CurrentUserId)
                : _visitPassService.GetAllWithDetails();

            return View(visitPasses);
        }

        [Authorize(Roles = "Admin,HomeOwner")]
        public async Task<IActionResult> Create()
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
                await PopulateSelectedHomeOwnerNameAsync(home.OwnerUserId);
                return View(new AccessViewModel
                {
                    EstateName = home.Estate.EstateName,
                    OwnerUserId = home.OwnerUserId,
                    Address = home.Address
                });
            }

            await PopulateHomeOwnerUsersAsync();
            PopulateVisitors();
            return View(new AccessViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HomeOwner")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                await PopulateSelectedHomeOwnerNameAsync(User.IsInRole("HomeOwner") ? CurrentUserId : model.OwnerUserId);
                return View(model);
            }

            model.EstateName = model.EstateName.Trim();
            model.Address = model.Address.Trim();
            string ownerUserId = User.IsInRole("HomeOwner") ? CurrentUserId : model.OwnerUserId;
            Home? home;

            if (User.IsInRole("HomeOwner"))
            {
                home = _homeService.GetByOwnerUserId(CurrentUserId);

                if (home == null)
                {
                    ModelState.AddModelError(string.Empty, "No home is assigned to your account yet.");
                    await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                    PopulateVisitors(model.VisitorId);
                    return View(model);
                }

                model.OwnerUserId = home.OwnerUserId;
                model.Address = home.Address;
                model.EstateName = home.Estate.EstateName;
            }
            else
            {
                if (!await IsValidHomeOwnerUserAsync(ownerUserId))
                {
                    ModelState.AddModelError(nameof(model.OwnerUserId), "Select a valid homeowner account.");
                    await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                    PopulateVisitors(model.VisitorId);
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

                home = _homeService.GetByAddressAndEstateId(model.Address, estate.EstateId);

                if (_homeService.ExistsByOwnerUserId(ownerUserId, home?.HomeId))
                {
                    ModelState.AddModelError(nameof(model.OwnerUserId), "This homeowner is already assigned to another home.");
                    await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                    PopulateVisitors(model.VisitorId);
                    return View(model);
                }

            }

            Visitor? visitor = _visitorService.GetById(model.VisitorId);

            if (visitor == null)
            {
                ModelState.AddModelError(nameof(model.VisitorId), "Select a valid visitor.");
                PopulateVisitors();
                return View(model);
            }

            if (!User.IsInRole("HomeOwner"))
            {
                Estate? estate = _estateService.GetByName(model.EstateName);

                if (estate == null)
                {
                    return View(model);
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
                else if (home.OwnerUserId != ownerUserId)
                {
                    home.OwnerUserId = ownerUserId;
                    _homeService.Update(home);
                }
            }

            if (home == null)
            {
                ModelState.AddModelError(string.Empty, "The selected home could not be found.");
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            int homeId = home.HomeId;

            if (_visitPassService.HasActivePassForVisitorAndHome(visitor.VisitorId, homeId))
            {
                ModelState.AddModelError(nameof(model.VisitorId), "This visitor already has an active visit pass for the selected home.");
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
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
                HomeId = homeId,
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
        public async Task<IActionResult> Edit(int id)
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

            await PopulateHomeOwnerUsersAsync(visitPass.Home.OwnerUserId);
            await PopulateSelectedHomeOwnerNameAsync(visitPass.Home.OwnerUserId);

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AccessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                await PopulateSelectedHomeOwnerNameAsync(User.IsInRole("HomeOwner") ? CurrentUserId : model.OwnerUserId);
                return View(model);
            }

            model.EstateName = model.EstateName.Trim();
            model.Address = model.Address.Trim();
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

            Home? home = _homeService.GetFullDetails(visitPass.HomeId);

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
                if (!await IsValidHomeOwnerUserAsync(ownerUserId))
                {
                    ModelState.AddModelError(nameof(model.OwnerUserId), "Select a valid homeowner account.");
                    await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                    PopulateVisitors(model.VisitorId);
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

                if (_homeService.ExistsByAddressAndEstateId(model.Address, estate.EstateId, home.HomeId))
                {
                    ModelState.AddModelError(nameof(model.Address), "A different home already uses this address in the selected estate.");
                    await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                    PopulateVisitors(model.VisitorId);
                    return View(model);
                }

                if (_homeService.ExistsByOwnerUserId(ownerUserId, home.HomeId))
                {
                    ModelState.AddModelError(nameof(model.OwnerUserId), "This homeowner is already assigned to another home.");
                    await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                    PopulateVisitors(model.VisitorId);
                    return View(model);
                }

                home.OwnerUserId = ownerUserId;
                home.Address = model.Address;
                home.EstateId = estate.EstateId;
            }

            Visitor? visitor = _visitorService.GetById(model.VisitorId);

            if (visitor == null)
            {
                ModelState.AddModelError(nameof(model.VisitorId), "Select a valid visitor.");
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            if (_visitPassService.HasActivePassForVisitorAndHome(visitor.VisitorId, visitPass.HomeId, visitPass.VisitPassId))
            {
                ModelState.AddModelError(nameof(model.VisitorId), "This visitor already has another active visit pass for the selected home.");
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            visitPass.VisitorId = visitor.VisitorId;
            visitPass.CodeHash = model.CodeHash;
            visitPass.CreatedByUserId = string.IsNullOrWhiteSpace(CurrentUserId) ? model.CreatedByUserId : CurrentUserId;
            if (!string.Equals(model.Status, PassStatuses.Active, StringComparison.Ordinal) &&
                !string.Equals(model.Status, PassStatuses.Revoked, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(model.Status), "Only Active or Revoked statuses can be set manually.");
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            if (string.Equals(model.Status, PassStatuses.Revoked, StringComparison.Ordinal))
            {
                _visitPassService.Revoke(visitPass);
            }
            else
            {
                visitPass.Status = PassStatuses.Active;
            }
            visitPass.CreatedAt = model.CreatedAt;
            visitPass.ExpiresAt = model.ExpiresAt;

            if (visitPass.ExpiresAt <= visitPass.CreatedAt)
            {
                ModelState.AddModelError(nameof(model.ExpiresAt), "Expiry must be later than the pass creation time.");
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            if (!User.IsInRole("HomeOwner"))
            {
                _homeService.Update(home);
            }

            _visitPassService.Update(visitPass);
            _visitPassService.NormalizeStatus(visitPass);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
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

            ApplicationUser? homeOwnerUser = await _userManager.FindByIdAsync(visitPass.Home.OwnerUserId);
            ViewBag.HomeOwnerName = homeOwnerUser?.FullName ?? homeOwnerUser?.Email ?? visitPass.Home.OwnerUserId;

            if (!string.IsNullOrWhiteSpace(visitPass.GateCheckIn?.SecurityUserId))
            {
                ApplicationUser? securityUser = await _userManager.FindByIdAsync(visitPass.GateCheckIn.SecurityUserId);
                ViewBag.SecurityUserName = securityUser?.FullName ?? securityUser?.Email ?? visitPass.GateCheckIn.SecurityUserId;
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
                model.StatusLabel = "Missing Code";
                model.Message = "Enter a pass code to verify.";
                return View(model);
            }

            string enteredCode = model.EnteredCode.Trim().ToUpperInvariant();
            List<VisitPass> visitPasses = _visitPassService.GetAllWithDetails();
            VisitPass? matchedPass = visitPasses.FirstOrDefault(v => _visitPassService.VerifyCode(v, enteredCode));

            if (matchedPass == null)
            {
                model.StatusLabel = "Wrong Code";
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
                model.StatusLabel = PassStatuses.Used;
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

            model.StatusLabel = currentStatus;
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

        [HttpPost]
        [Authorize(Roles = "Admin,HomeOwner")]
        [ValidateAntiForgeryToken]
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
