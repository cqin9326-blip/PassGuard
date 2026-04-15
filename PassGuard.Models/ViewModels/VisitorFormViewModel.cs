using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models.ViewModels
{
    public class VisitorFormViewModel
    {
        public int VisitorId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required]
        [StringLength(30)]
        [Phone]
        public string Phone { get; set; } = "";
    }
}
