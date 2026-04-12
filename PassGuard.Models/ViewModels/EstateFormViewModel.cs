using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models.ViewModels
{
    public class EstateFormViewModel
    {
        public int EstateId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Estate Name")]
        public string EstateName { get; set; } = "";
    }
}
