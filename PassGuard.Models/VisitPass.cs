namespace PassGuard.Models
{
    public class VisitPass
    {
        public int VisitPassId { get; set; }

        public int VisitorId { get; set; }
        public Visitor Visitor { get; set; } = null!;
        public string CodeHash { get; set; } = "";
        public string CreatedByUserId { get; set; } = "";
        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public int HomeId { get; set; }
        public Home Home { get; set; } = null!;

        public GateCheckIn? GateCheckIn { get; set; }
    }
}
