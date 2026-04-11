using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PassGuard.Models;

namespace PassGuard.DAL
{
    public class HomeRepository
    {
        private readonly PassGuardContext _context;

        public HomeRepository(PassGuardContext context)
        {
            _context = context;
        }

        public List<Home> GetAllWithDetails()
        {
            return _context.Homes
                .Include(h => h.Estate)
                .Include(h => h.VisitPasses)
                .OrderBy(h => h.OwnerUserId)
                .ToList();
        }

        public Home? GetFullDetails(int id)
        {
            return _context.Homes
                .Include(h => h.Estate)
                .Include(h => h.VisitPasses)
                    .ThenInclude(v => v.Visitor)
                .Include(h => h.VisitPasses)
                    .ThenInclude(v => v.GateCheckIn)
                .FirstOrDefault(h => h.HomeId == id);
        }

        public Home? GetById(int id)
        {
            return _context.Homes.FirstOrDefault(h => h.HomeId == id);
        }

        public Home? GetByAddressAndEstateId(string address, int estateId)
        {
            return _context.Homes.FirstOrDefault(h => h.Address == address && h.EstateId == estateId);
        }

        public Home? GetByOwnerUserId(string ownerUserId)
        {
            return _context.Homes
                .Include(h => h.Estate)
                .Include(h => h.VisitPasses)
                    .ThenInclude(v => v.Visitor)
                .Include(h => h.VisitPasses)
                    .ThenInclude(v => v.GateCheckIn)
                .FirstOrDefault(h => h.OwnerUserId == ownerUserId);
        }

        public void Add(Home home)
        {
            _context.Homes.Add(home);
            _context.SaveChanges();
        }

        public void Update(Home home)
        {
            _context.Homes.Update(home);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            Home? home = _context.Homes.FirstOrDefault(h => h.HomeId == id);

            if (home != null)
            {
                _context.Homes.Remove(home);
                _context.SaveChanges();
            }
        }

        public int Count()
        {
            return _context.Homes.Count();
        }
    }
}
