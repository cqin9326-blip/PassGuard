namespace PassGuard.Models
{
    public class GateCheckIn
    {
        public int GateCheckInId { get; set; }

        public string Result { get; set; } = "";
        public DateTime CheckInTime { get; set; }
        public string Note { get; set; } = "";

        public int VisitPassId { get; set; }
        public VisitPass VisitPass { get; set; } = null!;
    }
}