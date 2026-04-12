using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models
{
    public class GateCheckIn
    {
        public int GateCheckInId { get; set; }

        [Required]
        [StringLength(30)]
        public string Result { get; set; } = "";
        public DateTime CheckInTime { get; set; }

        [StringLength(300)]
        public string Note { get; set; } = "";

        [Required]
        [StringLength(450)]
        public string SecurityUserId { get; set; } = "";

        [Range(1, int.MaxValue)]
        public int VisitPassId { get; set; }
        public VisitPass VisitPass { get; set; } = null!;
    }
}
