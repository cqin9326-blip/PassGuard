using PassGuard.Models;

namespace PassGuard.DAL
{
    public class AuditLogRepository
    {
        private readonly PassGuardContext _context;

        public AuditLogRepository(PassGuardContext context)
        {
            _context = context;
        }

        public void Add(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            _context.SaveChanges();
        }

        public List<AuditLog> GetRecent(int count = 50)
        {
            return _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }
    }
}
