using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassGuard.BLL;
using PassGuard.Models;

namespace PassGuard.Controllers
{
    [Authorize(Roles = "Admin,Security")]
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
            ViewBag.SecurityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            return View();
        }

        [HttpPost]
        public IActionResult Create(int visitPassId, string result, DateTime checkInTime, string note, string securityUserId)
        {
            GateCheckIn gateCheckIn = new GateCheckIn
            {
                VisitPassId = visitPassId,
                Result = result,
                CheckInTime = checkInTime,
                Note = note,
                SecurityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? (string.IsNullOrWhiteSpace(securityUserId) ? "system" : securityUserId)
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

            gateCheckIn.SecurityUserId ??= User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            return View(gateCheckIn);
        }

        [HttpPost]
        public IActionResult Edit(GateCheckIn model)
        {
            if (User.IsInRole("Security"))
            {
                model.SecurityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? model.SecurityUserId;
            }

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
