namespace PassGuard.Models
{
    public class Home
    {
        public int HomeId { get; set; }

        public string OwnerName { get; set; } = "";
        public string Address { get; set; } = "";

        public int EstateId { get; set; }
        public Estate Estate { get; set; } = null!;

        public List<VisitPass> VisitPasses { get; set; } = new List<VisitPass>();
    }
}