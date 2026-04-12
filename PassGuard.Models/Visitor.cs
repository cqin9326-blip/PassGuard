using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models
{
    public class Visitor
    {
        public int VisitorId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = "";

        [Required]
        [StringLength(30)]
        [Phone]
        public string Phone { get; set; } = "";

        public List<VisitPass> VisitPasses { get; set; } = new List<VisitPass>();
    }
}
