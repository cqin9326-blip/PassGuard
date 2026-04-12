using PassGuard.DAL;
using PassGuard.Models;

namespace PassGuard.BLL
{
    public class AuditLogService
    {
        private readonly AuditLogRepository _repo;

        public AuditLogService(AuditLogRepository repo)
        {
            _repo = repo;
        }

        public void Log(string actionType, string entityType, string entityId, string userId, string userEmail, string detail)
        {
            _repo.Add(new AuditLog
            {
                Timestamp = DateTime.Now,
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                UserEmail = userEmail,
                Detail = detail
            });
        }

        public List<AuditLog> GetRecent(int count = 50)
        {
            return _repo.GetRecent(count);
        }
    }
}
