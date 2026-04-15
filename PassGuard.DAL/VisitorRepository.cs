using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            return _context.Visitors
                .Include(v => v.VisitPasses)
                .FirstOrDefault(v => v.VisitorId == id);
        }

        public Visitor? GetByFullNameAndPhone(string fullName, string phone)
        {
            string normalizedFullName = fullName.Trim();
            string normalizedPhone = phone.Trim();

            return _context.Visitors.FirstOrDefault(v => v.FullName == normalizedFullName && v.Phone == normalizedPhone);
        }

        public Visitor? GetByIdForCreator(int id, string createdByUserId)
        {
            return _context.Visitors
                .Include(v => v.VisitPasses)
                .FirstOrDefault(v => v.VisitorId == id && v.CreatedByUserId == createdByUserId);
        }

        public List<Visitor> GetAll()
        {
            return _context.Visitors
                .Include(v => v.VisitPasses)
                .OrderBy(v => v.FullName)
                .ToList();
        }

        public List<Visitor> GetByCreatedByUserId(string createdByUserId)
        {
            return _context.Visitors
                .Include(v => v.VisitPasses)
                .Where(v => v.CreatedByUserId == createdByUserId)
                .OrderBy(v => v.FullName)
                .ToList();
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

        public void Delete(int id)
        {
            Visitor? visitor = _context.Visitors.FirstOrDefault(v => v.VisitorId == id);

            if (visitor != null)
            {
                _context.Visitors.Remove(visitor);
                _context.SaveChanges();
            }
        }

        public bool ExistsByFullNameAndPhone(string fullName, string phone, int? ignoreVisitorId = null)
        {
            string normalizedFullName = fullName.Trim();
            string normalizedPhone = phone.Trim();

            return _context.Visitors.Any(v =>
                v.FullName == normalizedFullName &&
                v.Phone == normalizedPhone &&
                (!ignoreVisitorId.HasValue || v.VisitorId != ignoreVisitorId.Value));
        }

        public bool ExistsByFullNameAndPhoneForCreator(string fullName, string phone, string createdByUserId, int? ignoreVisitorId = null)
        {
            string normalizedFullName = fullName.Trim();
            string normalizedPhone = phone.Trim();

            return _context.Visitors.Any(v =>
                v.CreatedByUserId == createdByUserId &&
                v.FullName == normalizedFullName &&
                v.Phone == normalizedPhone &&
                (!ignoreVisitorId.HasValue || v.VisitorId != ignoreVisitorId.Value));
        }
    }
}
