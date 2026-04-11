namespace PassGuard.Models.ViewModels
{
    public class AccessViewModel
    {
        public int VisitPassId { get; set; }
        public int HomeId { get; set; }
        public int? GateCheckInId { get; set; }
        public string EstateName { get; set; } = "";
        public string OwnerUserId { get; set; } = "";
        public string Address { get; set; } = "";
        public string VisitorFullName { get; set; } = "";
        public string VisitorPhone { get; set; } = "";
        public string CodeHash { get; set; } = "";
        public string CreatedByUserId { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string SecurityUserId { get; set; } = "";
        public string CheckInResult { get; set; } = "";
        public DateTime? CheckInTime { get; set; }
        public string CheckInNote { get; set; } = "";
    }
}
