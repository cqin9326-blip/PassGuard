namespace PassGuard.Models.ViewModels
{
    public class AuditLogListItemViewModel
    {
        public DateTime Timestamp { get; set; }
        public string ActionType { get; set; } = "";
        public string EntityType { get; set; } = "";
        public string EntityId { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string Detail { get; set; } = "";
    }
}
