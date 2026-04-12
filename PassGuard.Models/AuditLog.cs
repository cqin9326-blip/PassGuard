namespace PassGuard.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public DateTime Timestamp { get; set; }
        public string ActionType { get; set; } = "";
        public string EntityType { get; set; } = "";
        public string EntityId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string Detail { get; set; } = "";
    }
}
