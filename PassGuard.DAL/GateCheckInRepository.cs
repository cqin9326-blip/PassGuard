using System.Collections.Generic;
using System.Linq;
using PassGuard.Models;

namespace PassGuard.DAL
{
    public class GateCheckInRepository
    {
        private readonly PassGuardContext _context;

        public GateCheckInRepository(PassGuardContext context)
        {
            _context = context;
        }

        public GateCheckIn? GetByVisitPassId(int visitPassId)
        {
            return _context.GateCheckIns
                .Where(g => g.VisitPassId == visitPassId)
                .FirstOrDefault();
        }

        public GateCheckIn? GetById(int id)
        {
            return _context.GateCheckIns.FirstOrDefault(g => g.GateCheckInId == id);
        }

        public void Add(GateCheckIn gateCheckIn)
        {
            _context.GateCheckIns.Add(gateCheckIn);
            _context.SaveChanges();
        }

        public void Update(GateCheckIn gateCheckIn)
        {
            _context.GateCheckIns.Update(gateCheckIn);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            GateCheckIn? gateCheckIn = _context.GateCheckIns.FirstOrDefault(g => g.GateCheckInId == id);

            if (gateCheckIn != null)
            {
                _context.GateCheckIns.Remove(gateCheckIn);
                _context.SaveChanges();
            }
        }

        public int GetTodayCount()
        {
            DateTime today = DateTime.Today;

            return _context.GateCheckIns.Count(g => g.CheckInTime.Date == today);
        }
    }
}
