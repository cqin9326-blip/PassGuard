using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models
{
    public class Home
    {
        public int HomeId { get; set; }

        [Required]
        [StringLength(450)]
        public string OwnerUserId { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = "";

        [Range(1, int.MaxValue)]
        public int EstateId { get; set; }
        public Estate Estate { get; set; } = null!;

        public List<VisitPass> VisitPasses { get; set; } = new List<VisitPass>();
    }
}
