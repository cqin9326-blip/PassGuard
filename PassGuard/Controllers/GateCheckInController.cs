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
        private readonly VisitPassService _visitPassService;
        private readonly AuditLogService _auditLogService;

        public GateCheckInController(
            GateCheckInService gateCheckInService,
            VisitPassService visitPassService,
            AuditLogService auditLogService)
        {
            _gateCheckInService = gateCheckInService;
            _visitPassService = visitPassService;
            _auditLogService = auditLogService;
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
            VisitPass? visitPass = _visitPassService.GetFullDetails(visitPassId);

            if (visitPass == null)
            {
                return NotFound();
            }

            if (string.Equals(result, "Approved", StringComparison.OrdinalIgnoreCase) && !_visitPassService.CanBeAccepted(visitPass))
            {
                TempData["ErrorMessage"] = $"This pass is {visitPass.Status} and cannot be approved.";
                return RedirectToAction("Details", "VisitPass", new { id = visitPassId });
            }

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
            UpdatePassStatus(visitPassId);
            _auditLogService.Log(
                "Check-In Result",
                "GateCheckIn",
                gateCheckIn.GateCheckInId.ToString(),
                gateCheckIn.SecurityUserId,
                User.Identity?.Name ?? "",
                $"Recorded {gateCheckIn.Result} check-in for pass {visitPassId}.");

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
            VisitPass? visitPass = _visitPassService.GetFullDetails(model.VisitPassId);

            if (visitPass == null)
            {
                return NotFound();
            }

            if (string.Equals(model.Result, "Approved", StringComparison.OrdinalIgnoreCase) && !_visitPassService.CanBeAccepted(visitPass))
            {
                TempData["ErrorMessage"] = $"This pass is {visitPass.Status} and cannot be approved.";
                return RedirectToAction("Details", "VisitPass", new { id = model.VisitPassId });
            }

            if (User.IsInRole("Security"))
            {
                model.SecurityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? model.SecurityUserId;
            }

            _gateCheckInService.Update(model);
            UpdatePassStatus(model.VisitPassId);
            _auditLogService.Log(
                "Check-In Result",
                "GateCheckIn",
                model.GateCheckInId.ToString(),
                model.SecurityUserId,
                User.Identity?.Name ?? "",
                $"Updated check-in to {model.Result} for pass {model.VisitPassId}.");
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
            UpdatePassStatus(visitPassId);

            return RedirectToAction("Details", "VisitPass", new { id = visitPassId });
        }

        private void UpdatePassStatus(int visitPassId)
        {
            VisitPass? visitPass = _visitPassService.GetFullDetails(visitPassId);

            if (visitPass != null)
            {
                _visitPassService.NormalizeStatus(visitPass);
            }
        }
    }
}
