using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.Models;

namespace PassGuard.Controllers
{
    public class GateCheckInController : Controller
    {
        private readonly GateCheckInService _gateCheckInService;

        public GateCheckInController(GateCheckInService gateCheckInService)
        {
            _gateCheckInService = gateCheckInService;
        }

        public IActionResult Create(int visitPassId)
        {
            ViewBag.VisitPassId = visitPassId;
            return View();
        }

        [HttpPost]
        public IActionResult Create(int visitPassId, string result, DateTime checkInTime, string note)
        {
            GateCheckIn gateCheckIn = new GateCheckIn
            {
                VisitPassId = visitPassId,
                Result = result,
                CheckInTime = checkInTime,
                Note = note
            };

            _gateCheckInService.Add(gateCheckIn);

            return RedirectToAction("Details", "VisitPass", new { id = visitPassId });
        }

        public IActionResult Edit(int id)
        {
            GateCheckIn? gateCheckIn = _gateCheckInService.GetById(id);

            if (gateCheckIn == null)
            {
                return NotFound();
            }

            return View(gateCheckIn);
        }

        [HttpPost]
        public IActionResult Edit(GateCheckIn model)
        {
            _gateCheckInService.Update(model);
            return RedirectToAction("Details", "VisitPass", new { id = model.VisitPassId });
        }

        public IActionResult Delete(int id)
        {
            GateCheckIn? gateCheckIn = _gateCheckInService.GetById(id);

            if (gateCheckIn == null)
            {
                return RedirectToAction("Index", "VisitPass");
            }

            int visitPassId = gateCheckIn.VisitPassId;
            _gateCheckInService.Delete(id);

            return RedirectToAction("Details", "VisitPass", new { id = visitPassId });
        }
    }
}