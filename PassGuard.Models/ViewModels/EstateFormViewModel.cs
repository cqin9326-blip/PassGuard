using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models.ViewModels
{
    public class EstateFormViewModel
    {
        public int EstateId { get; set; }

        [Required]
        public string EstateName { get; set; } = "";
    }
}
