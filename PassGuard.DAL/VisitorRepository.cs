using System.Linq;
using PassGuard.Models;

namespace PassGuard.DAL
{
    public class VisitorRepository
    {
        private readonly PassGuardContext _context;

        public VisitorRepository(PassGuardContext context)
        {
            _context = context;
        }

        public Visitor? GetById(int id)
        {
            return _context.Visitors.FirstOrDefault(v => v.VisitorId == id);
        }

        public Visitor? GetByFullNameAndPhone(string fullName, string phone)
        {
            return _context.Visitors.FirstOrDefault(v => v.FullName == fullName && v.Phone == phone);
        }

        public void Add(Visitor visitor)
        {
            _context.Visitors.Add(visitor);
            _context.SaveChanges();
        }

        public void Update(Visitor visitor)
        {
            _context.Visitors.Update(visitor);
            _context.SaveChanges();
        }
    }
}
