using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PassGuard.Models;

namespace PassGuard.DAL
{
    public class VisitPassRepository
    {
        private readonly PassGuardContext _context;

        public VisitPassRepository(PassGuardContext context)
        {
            _context = context;
        }
        public List<VisitPass> GetAllWithDetails()
        {
            return _context.VisitPasses
                .Include(v => v.Visitor)
                .Include(v => v.Home)
                    .ThenInclude(h => h.Estate)
                .Include(v => v.GateCheckIn)
                .OrderByDescending(v => v.CreatedAt)
                .ToList();
        }
        public VisitPass? GetFullDetails(int id)
        {
            return _context.VisitPasses
                .Include(v => v.Visitor)
                .Include(v => v.Home)
                    .ThenInclude(h => h.Estate)
                .Include(v => v.GateCheckIn)
                .FirstOrDefault(v => v.VisitPassId == id);
        }

        public VisitPass? GetById(int id)
        {
            return _context.VisitPasses.FirstOrDefault(v => v.VisitPassId == id);
        }

        public VisitPass? GetFirstByHomeId(int homeId)
        {
            return _context.VisitPasses.FirstOrDefault(v => v.HomeId == homeId);
        }

        public List<VisitPass> GetByCreatedByUserId(string createdByUserId)
        {
            return _context.VisitPasses
                .Include(v => v.Visitor)
                .Include(v => v.Home)
                    .ThenInclude(h => h.Estate)
                .Include(v => v.GateCheckIn)
                .Where(v => v.CreatedByUserId == createdByUserId)
                .OrderByDescending(v => v.CreatedAt)
                .ToList();
        }

        public void Add(VisitPass visitPass)
        {
            _context.VisitPasses.Add(visitPass);
            _context.SaveChanges();
        }

        public void Update(VisitPass visitPass)
        {
            _context.VisitPasses.Update(visitPass);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            VisitPass? visitPass = _context.VisitPasses.FirstOrDefault(v => v.VisitPassId == id);

            if (visitPass != null)
            {
                _context.VisitPasses.Remove(visitPass);
                _context.SaveChanges();
            }
        }

        public int CountDistinctVisitors()
        {
            return _context.VisitPasses
                .Select(v => v.VisitorId)
                .Distinct()
                .Count();
        }

        public int CountActivePasses()
        {
            return _context.VisitPasses.Count(v => v.Status == "Active");
        }

        public bool HasActivePassForVisitorAndHome(int visitorId, int homeId, int? ignoreVisitPassId = null)
        {
            DateTime now = DateTime.Now;

            return _context.VisitPasses.Any(v =>
                v.VisitorId == visitorId &&
                v.HomeId == homeId &&
                v.ExpiresAt > now &&
                v.Status == PassStatuses.Active &&
                (!ignoreVisitPassId.HasValue || v.VisitPassId != ignoreVisitPassId.Value));
        }
    }
}
