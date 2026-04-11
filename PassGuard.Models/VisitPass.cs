namespace PassGuard.Models
{
    public class VisitPass
    {
        public int VisitPassId { get; set; }

        public string VisitorName { get; set; } = "";
        public string VisitorPhone { get; set; } = "";

        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public int HomeId { get; set; }
        public Home Home { get; set; } = null!;

        public List<GateCheckIn> GateCheckIns { get; set; } = new List<GateCheckIn>();
    }
}