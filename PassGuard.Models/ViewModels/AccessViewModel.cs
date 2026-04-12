using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models.ViewModels
{
    public class AccessViewModel
    {
        public int VisitPassId { get; set; }
        public int HomeId { get; set; }
        public int? GateCheckInId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Estate Name")]
        public string EstateName { get; set; } = "";

        [Required]
        [Display(Name = "Homeowner")]
        public string OwnerUserId { get; set; } = "";

        [Required]
        [StringLength(200)]
        [Display(Name = "Home Address")]
        public string Address { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Select a visitor.")]
        [Display(Name = "Visitor")]
        public int VisitorId { get; set; }
        public string CodeHash { get; set; } = "";
        public string CreatedByUserId { get; set; } = "";

        [StringLength(30)]
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Expires At")]
        public DateTime ExpiresAt { get; set; }
        public string SecurityUserId { get; set; } = "";

        [StringLength(30)]
        [Display(Name = "Check-In Result")]
        public string CheckInResult { get; set; } = "";

        [Display(Name = "Check-In Time")]
        public DateTime? CheckInTime { get; set; }

        [StringLength(300)]
        [Display(Name = "Check-In Notes")]
        public string CheckInNote { get; set; } = "";
    }
}
