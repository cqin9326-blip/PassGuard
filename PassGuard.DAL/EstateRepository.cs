using System.Linq;
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
            return _context.Estates.FirstOrDefault(e => e.EstateId == id);
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

        public int Count()
        {
            return _context.Estates.Count();
        }
    }
}