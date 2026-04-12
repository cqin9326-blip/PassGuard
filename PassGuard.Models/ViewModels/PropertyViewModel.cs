using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models.ViewModels
{
    public class PropertyViewModel
    {
        public int HomeId { get; set; }

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

        [Required]
        [Display(Name = "Default Visitor")]
        public int? VisitorId { get; set; }
    }
}
