using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models
{
    public class VisitPass
    {
        public int VisitPassId { get; set; }

        [Range(1, int.MaxValue)]
        public int VisitorId { get; set; }
        public Visitor Visitor { get; set; } = null!;

        [Required]
        [StringLength(256)]
        public string CodeHash { get; set; } = "";

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = "";

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        [Range(1, int.MaxValue)]
        public int HomeId { get; set; }
        public Home Home { get; set; } = null!;

        public GateCheckIn? GateCheckIn { get; set; }
    }
}
