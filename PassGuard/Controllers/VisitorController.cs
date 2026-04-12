using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.Models;
using PassGuard.Models.ViewModels;

namespace PassGuard.Controllers
{
    [Authorize]
    public class VisitorController : Controller
    {
        private readonly VisitorService _visitorService;

        public VisitorController(VisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View(_visitorService.GetAll());
        }

        [Authorize(Roles = "Admin,HomeOwner")]
        public IActionResult Create()
        {
            return View(new VisitorFormViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HomeOwner")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VisitorFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.FullName = model.FullName.Trim();
            model.Phone = model.Phone.Trim();

            if (_visitorService.ExistsByFullNameAndPhone(model.FullName, model.Phone))
            {
                ModelState.AddModelError(string.Empty, "This visitor already exists.");
                return View(model);
            }

            _visitorService.Add(new Visitor
            {
                FullName = model.FullName,
                Phone = model.Phone
            });

            TempData["SuccessMessage"] = "Visitor created successfully.";

            if (User.IsInRole("HomeOwner"))
            {
                return RedirectToAction("Create", "VisitPass");
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            Visitor? visitor = _visitorService.GetById(id);

            if (visitor == null)
            {
                return NotFound();
            }

            return View(new VisitorFormViewModel
            {
                VisitorId = visitor.VisitorId,
                FullName = visitor.FullName,
                Phone = visitor.Phone
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(VisitorFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.FullName = model.FullName.Trim();
            model.Phone = model.Phone.Trim();

            Visitor? visitor = _visitorService.GetById(model.VisitorId);

            if (visitor == null)
            {
                return NotFound();
            }

            if (_visitorService.ExistsByFullNameAndPhone(model.FullName, model.Phone, model.VisitorId))
            {
                ModelState.AddModelError(string.Empty, "This visitor already exists.");
                return View(model);
            }

            visitor.FullName = model.FullName;
            visitor.Phone = model.Phone;
            _visitorService.Update(visitor);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Details(int id)
        {
            Visitor? visitor = _visitorService.GetById(id);

            if (visitor == null)
            {
                return NotFound();
            }

            return View(visitor);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            Visitor? visitor = _visitorService.GetById(id);

            if (visitor == null)
            {
                return NotFound();
            }

            if (visitor.VisitPasses.Any())
            {
                TempData["ErrorMessage"] = "You cannot delete a visitor who already has visit pass history.";
                return RedirectToAction(nameof(Index));
            }

            _visitorService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
