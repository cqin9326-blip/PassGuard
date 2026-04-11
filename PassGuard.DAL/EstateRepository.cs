using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PassGuard.Models;

namespace PassGuard.DAL
{
    public class EstateRepository
    {
        private readonly PassGuardContext _context;

        public EstateRepository(PassGuardContext context)
        {
            _context = context;
        }

        public Estate? GetByName(string name)
        {
            return _context.Estates.FirstOrDefault(e => e.EstateName == name);
        }

        public Estate? GetById(int id)
        {
            return _context.Estates
                .Include(e => e.Homes)
                .FirstOrDefault(e => e.EstateId == id);
        }

        public List<Estate> GetAll()
        {
            return _context.Estates
                .Include(e => e.Homes)
                .OrderBy(e => e.EstateName)
                .ToList();
        }

        public void Add(Estate estate)
        {
            _context.Estates.Add(estate);
            _context.SaveChanges();
        }

        public void Update(Estate estate)
        {
            _context.Estates.Update(estate);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            Estate? estate = _context.Estates.FirstOrDefault(e => e.EstateId == id);

            if (estate != null)
            {
                _context.Estates.Remove(estate);
                _context.SaveChanges();
            }
        }

        public int Count()
        {
            return _context.Estates.Count();
        }
    }
}
