namespace PassGuard.Models.ViewModels
{
    public class HomeOwnerDashboardViewModel
    {
        public int ActivePassCount { get; set; }
        public int PastPassCount { get; set; }
        public int RevokedPassCount { get; set; }
        public List<VisitPass> ActivePasses { get; set; } = new List<VisitPass>();
        public List<VisitPass> VisitHistory { get; set; } = new List<VisitPass>();
    }
}
