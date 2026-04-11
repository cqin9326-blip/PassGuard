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

            Estate? estate = _estateService.GetById(model.EstateId);

            if (estate == null)
            {
                return NotFound();
            }

            estate.EstateName = model.EstateName;
            _estateService.Update(estate);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            Estate? estate = _estateService.GetById(id);

            if (estate == null)
            {
                return NotFound();
            }

            return View(estate);
        }

        public IActionResult Delete(int id)
        {
            _estateService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
