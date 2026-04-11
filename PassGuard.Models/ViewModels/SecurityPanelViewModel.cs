namespace PassGuard.Models.ViewModels
{
    public class SecurityPanelViewModel
    {
        public int ExpectedVisitorCount { get; set; }
        public int CheckedInTodayCount { get; set; }
        public List<VisitPass> ExpectedVisitors { get; set; } = new List<VisitPass>();
        public List<VisitPass> RecentChecks { get; set; } = new List<VisitPass>();
    }
}
