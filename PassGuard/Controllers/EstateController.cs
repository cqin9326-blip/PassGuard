using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.Models;
using PassGuard.Models.ViewModels;

namespace PassGuard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EstateController : Controller
    {
        private readonly EstateService _estateService;

        public EstateController(EstateService estateService)
        {
            _estateService = estateService;
        }

        public IActionResult Index()
        {
            List<Estate> estates = _estateService.GetAll();
            return View(estates);
        }

        public IActionResult Create()
        {
            return View(new EstateFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EstateFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.EstateName = model.EstateName.Trim();

            if (_estateService.ExistsByName(model.EstateName))
            {
                ModelState.AddModelError(nameof(model.EstateName), "An estate with this name already exists.");
                return View(model);
            }

            _estateService.Add(new Estate { EstateName = model.EstateName });
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Estate? estate = _estateService.GetById(id);

            if (estate == null)
            {
                return NotFound();
            }

            return View(new EstateFormViewModel
            {
                EstateId = estate.EstateId,
                EstateName = estate.EstateName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EstateFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.EstateName = model.EstateName.Trim();

            Estate? estate = _estateService.GetById(model.EstateId);

            if (estate == null)
            {
                return NotFound();
            }

            if (_estateService.ExistsByName(model.EstateName, model.EstateId))
            {
                ModelState.AddModelError(nameof(model.EstateName), "An estate with this name already exists.");
                return View(model);
            }

            estate.EstateName = model.EstateName;
            _estateService.Update(estate);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            Estate? estate = _estateService.GetFullDetails(id);

            if (estate == null)
            {
                return NotFound();
            }

            DateTime today = DateTime.Today;
            List<VisitPass> visitPasses = estate.Homes.SelectMany(h => h.VisitPasses).ToList();

            EstateDetailsViewModel model = new EstateDetailsViewModel
            {
                EstateId = estate.EstateId,
                EstateName = estate.EstateName,
                HomeCount = estate.Homes.Count,
                VisitPassCount = visitPasses.Count,
                ActivePassCount = visitPasses.Count(v => v.Status == PassStatuses.Active),
                CheckedInTodayCount = visitPasses.Count(v => v.GateCheckIn != null && v.GateCheckIn.CheckInTime.Date == today),
                Homes = estate.Homes.OrderBy(h => h.Address).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            Estate? estate = _estateService.GetById(id);

            if (estate == null)
            {
                return NotFound();
            }

            if (estate.Homes.Any())
            {
                TempData["ErrorMessage"] = "You cannot delete an estate that still has homes assigned to it.";
                return RedirectToAction(nameof(Index));
            }

            _estateService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
