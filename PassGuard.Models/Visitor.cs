namespace PassGuard.Models
{
    public class Visitor
    {
        public int VisitorId { get; set; }

        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";

        public List<VisitPass> VisitPasses { get; set; } = new List<VisitPass>();
    }
}
