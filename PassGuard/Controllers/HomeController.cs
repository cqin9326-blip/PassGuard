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
        private readonly VisitPassService _visitPassService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            HomeService homeService,
            EstateService estateService,
            VisitPassService visitPassService,
            UserManager<ApplicationUser> userManager)
        {
            _homeService = homeService;
            _estateService = estateService;
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

        private void PopulateEstates(string? selectedEstateName = null)
        {
            ViewBag.Estates = _estateService.GetAll()
                .OrderBy(e => e.EstateName)
                .Select(e => new SelectListItem
                {
                    Value = e.EstateName,
                    Text = e.EstateName,
                    Selected = string.Equals(e.EstateName, selectedEstateName, StringComparison.Ordinal)
                })
                .ToList();
        }

        private async Task<bool> IsValidHomeOwnerUserAsync(string ownerUserId)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(ownerUserId);
            return user != null && await _userManager.IsInRoleAsync(user, "HomeOwner");
        }

        public async Task<IActionResult> Index(int? estateId = null)
        {
            List<Home> homes = estateId.HasValue
                ? _homeService.GetByEstateIdWithDetails(estateId.Value)
                : _homeService.GetAllWithDetails();

            List<Estate> estates = _estateService.GetAll();

            HomeIndexViewModel model = new HomeIndexViewModel
            {
                SelectedEstateId = estateId,
                SelectedEstateName = estateId.HasValue
                    ? estates.FirstOrDefault(e => e.EstateId == estateId.Value)?.EstateName ?? ""
                    : "",
                Estates = estates
                    .Select(e => new HomeEstateOptionViewModel
                    {
                        EstateId = e.EstateId,
                        EstateName = e.EstateName,
                        Selected = estateId.HasValue && e.EstateId == estateId.Value
                    })
                    .ToList(),
                Homes = homes
            };

            ViewBag.HomeOwnerNames = await GetHomeOwnerNamesAsync(homes.Select(h => h.OwnerUserId));
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateHomeOwnerUsersAsync();
            PopulateEstates();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateEstates(model.EstateName);
                return View(model);
            }
            model.EstateName = model.EstateName.Trim();
            model.Address = model.Address.Trim();

            if (!await IsValidHomeOwnerUserAsync(model.OwnerUserId))
            {
                ModelState.AddModelError(nameof(model.OwnerUserId), "Select a valid homeowner account.");
            }

            Estate? estate = _estateService.GetByName(model.EstateName);

            if (estate == null)
            {
                ModelState.AddModelError(nameof(model.EstateName), "Select an estate from the list.");
            }

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
                PopulateEstates(model.EstateName);
                return View(model);
            }

            Home home = new Home
            {
                OwnerUserId = model.OwnerUserId,
                Address = model.Address,
                EstateId = estate.EstateId
            };

            _homeService.Add(home);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            Home? home = _homeService.GetFullDetails(id);

            if (home == null)
            {
                return NotFound();
            }

            PropertyViewModel model = new PropertyViewModel
            {
                HomeId = home.HomeId,
                EstateName = home.Estate.EstateName,
                OwnerUserId = home.OwnerUserId,
                Address = home.Address
            };

            await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
            PopulateEstates(model.EstateName);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateEstates(model.EstateName);
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

            if (estate == null)
            {
                ModelState.AddModelError(nameof(model.EstateName), "Select an estate from the list.");
            }

            if (estate != null && _homeService.ExistsByAddressAndEstateId(model.Address, estate.EstateId, model.HomeId))
            {
                ModelState.AddModelError(nameof(model.Address), "A home with this address already exists in the selected estate.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateHomeOwnerUsersAsync(model.OwnerUserId);
                PopulateEstates(model.EstateName);
                return View(model);
            }

            home.OwnerUserId = model.OwnerUserId;
            home.Address = model.Address;
            home.EstateId = estate.EstateId;

            _homeService.Update(home);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            Home? home = _homeService.GetFullDetails(id);

            if (home == null)
            {
                return NotFound();
            }

            ApplicationUser? homeOwnerUser = await _userManager.FindByIdAsync(home.OwnerUserId);
            ViewBag.HomeOwnerName = homeOwnerUser?.FullName ?? homeOwnerUser?.Email ?? home.OwnerUserId;

            return View(home);
        }

        private async Task<Dictionary<string, string>> GetHomeOwnerNamesAsync(IEnumerable<string> ownerUserIds)
        {
            Dictionary<string, string> homeOwnerNames = new Dictionary<string, string>();

            foreach (string ownerUserId in ownerUserIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct())
            {
                ApplicationUser? user = await _userManager.FindByIdAsync(ownerUserId);
                homeOwnerNames[ownerUserId] = user?.FullName ?? user?.Email ?? ownerUserId;
            }

            return homeOwnerNames;
        }

        public async Task<IActionResult> Delete(int id, bool visitPassesRemoved = false)
        {
            Home? home = _homeService.GetFullDetails(id);

            if (home == null)
            {
                return NotFound();
            }

            ApplicationUser? homeOwnerUser = await _userManager.FindByIdAsync(home.OwnerUserId);

            HomeDeleteViewModel model = new HomeDeleteViewModel
            {
                HomeId = home.HomeId,
                EstateName = home.Estate.EstateName,
                Address = home.Address,
                HomeOwnerName = homeOwnerUser?.FullName ?? homeOwnerUser?.Email ?? home.OwnerUserId,
                VisitPassCount = home.VisitPasses.Count,
                VisitPassesRemoved = visitPassesRemoved
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVisitPasses(int id)
        {
            Home? home = _homeService.GetFullDetails(id);

            if (home == null)
            {
                return NotFound();
            }

            if (home.VisitPasses.Any())
            {
                _visitPassService.DeleteByHomeId(id);
            }

            TempData["SuccessMessage"] = "Attached visit passes were deleted. Confirm the home deletion to finish.";
            return RedirectToAction(nameof(Delete), new { id, visitPassesRemoved = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmDelete(int id)
        {
            Home? home = _homeService.GetFullDetails(id);

            if (home == null)
            {
                return NotFound();
            }

            if (home.VisitPasses.Any())
            {
                TempData["ErrorMessage"] = "Delete the attached visit passes before deleting this home.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _homeService.Delete(id);
            TempData["SuccessMessage"] = "Home deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
