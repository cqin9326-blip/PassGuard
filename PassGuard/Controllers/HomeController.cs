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
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly HomeService _homeService;
        private readonly EstateService _estateService;
        private readonly VisitorService _visitorService;
        private readonly VisitPassService _visitPassService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            HomeService homeService,
            EstateService estateService,
            VisitorService visitorService,
            VisitPassService visitPassService,
            UserManager<ApplicationUser> userManager)
        {
            _homeService = homeService;
            _estateService = estateService;
            _visitorService = visitorService;
            _visitPassService = visitPassService;
            _userManager = userManager;
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

        private async Task<bool> IsValidHomeOwnerUserAsync(string ownerUserId)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(ownerUserId);
            return user != null && await _userManager.IsInRoleAsync(user, "HomeOwner");
        }

        public IActionResult Index()
        {
            var homes = _homeService.GetAllWithDetails();
            return View(homes);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateHomeOwnerUsersAsync();
            PopulateVisitors();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                return View(model);
            }
            model.EstateName = model.EstateName.Trim();
            model.Address = model.Address.Trim();

            if (!await IsValidHomeOwnerUserAsync(model.OwnerUserId))
            {
                ModelState.AddModelError(nameof(model.OwnerUserId), "Select a valid homeowner account.");
            }

            if (!model.VisitorId.HasValue)
            {
                ModelState.AddModelError(nameof(model.VisitorId), "Select a visitor.");
            }

            Estate? estate = _estateService.GetByName(model.EstateName);

            if (estate != null && _homeService.ExistsByAddressAndEstateId(model.Address, estate.EstateId))
            {
                ModelState.AddModelError(nameof(model.Address), "A home with this address already exists in the selected estate.");
            }

            if (_homeService.ExistsByOwnerUserId(model.OwnerUserId))
            {
                ModelState.AddModelError(nameof(model.OwnerUserId), "This homeowner is already assigned to another home.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            int visitorId = model.VisitorId ?? 0;
            Visitor? visitor = _visitorService.GetById(visitorId);

            if (visitor == null)
            {
                ModelState.AddModelError(nameof(model.VisitorId), "Selected visitor was not found.");
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors();
                return View(model);
            }

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

            DateTime now = DateTime.Now;
            DateTime expire = now.AddDays(1);

            VisitPass visitPass = new VisitPass
            {
                VisitorId = visitor.VisitorId,
                CodeHash = _visitPassService.HashCode(_visitPassService.GeneratePlainCode()),
                CreatedByUserId = model.OwnerUserId,
                HomeId = home.HomeId,
                CreatedAt = now,
                ExpiresAt = expire,
                Status = PassStatuses.Active
            };

            _visitPassService.Add(visitPass);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
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
                VisitorId = visitPass?.VisitorId
            };

            await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
            PopulateVisitors(model.VisitorId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                return View(model);
            }
            model.EstateName = model.EstateName.Trim();
            model.Address = model.Address.Trim();

            Home? home = _homeService.GetById(model.HomeId);

            if (home == null)
            {
                return NotFound();
            }

            if (!await IsValidHomeOwnerUserAsync(model.OwnerUserId))
            {
                ModelState.AddModelError(nameof(model.OwnerUserId), "Select a valid homeowner account.");
            }

            if (_homeService.ExistsByOwnerUserId(model.OwnerUserId, model.HomeId))
            {
                ModelState.AddModelError(nameof(model.OwnerUserId), "This homeowner is already assigned to another home.");
            }

            Estate? estate = _estateService.GetByName(model.EstateName);

            if (estate != null && _homeService.ExistsByAddressAndEstateId(model.Address, estate.EstateId, model.HomeId))
            {
                ModelState.AddModelError(nameof(model.Address), "A home with this address already exists in the selected estate.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateVisitors(model.VisitorId);
                return View(model);
            }

            if (estate == null)
            {
                estate = new Estate
                {
                    EstateName = model.EstateName
                };
                _estateService.Add(estate);
            }

            VisitPass? visitPass = _visitPassService.GetFirstByHomeId(model.HomeId);

            if (visitPass != null)
            {
                if (!model.VisitorId.HasValue)
                {
                    ModelState.AddModelError(nameof(model.VisitorId), "Select a visitor.");
                    await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                    PopulateVisitors();
                    return View(model);
                }

                Visitor? visitor = _visitorService.GetById(model.VisitorId.Value);

                if (visitor == null)
                {
                    ModelState.AddModelError(nameof(model.VisitorId), "Selected visitor was not found.");
                    await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                    PopulateVisitors();
                    return View(model);
                }

                visitPass.VisitorId = visitor.VisitorId;
                visitPass.CreatedByUserId = model.OwnerUserId;
            }

            home.OwnerUserId = model.OwnerUserId;
            home.Address = model.Address;
            home.EstateId = estate.EstateId;

            _homeService.Update(home);

            if (visitPass != null)
            {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            Home? home = _homeService.GetFullDetails(id);

            if (home == null)
            {
                return NotFound();
            }

            if (home.VisitPasses.Any())
            {
                TempData["ErrorMessage"] = "You cannot delete a home that already has visit pass history.";
                return RedirectToAction(nameof(Index));
            }

            _homeService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
