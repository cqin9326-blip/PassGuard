using System.ComponentModel.DataAnnotations;

namespace PassGuard.Models
{
    public class Estate
    {
        public int EstateId { get; set; }

        [Required]
        [StringLength(100)]
        public string EstateName { get; set; } = "";

        public List<Home> Homes { get; set; } = new List<Home>();
    }
}
