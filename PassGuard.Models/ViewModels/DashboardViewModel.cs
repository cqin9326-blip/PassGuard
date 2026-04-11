namespace PassGuard.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEstates { get; set; }
        public int TotalHomes { get; set; }
        public int TotalVisitors { get; set; }
        public int ActivePasses { get; set; }
        public int TodayCheckInCount { get; set; }
        public List<DashboardVisitorRowViewModel> TodayVisitors { get; set; } = new List<DashboardVisitorRowViewModel>();
    }

    public class DashboardVisitorRowViewModel
    {
        public string VisitorName { get; set; } = "";
        public string HomeAddress { get; set; } = "";
        public string EstateName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; } = "";
        public string Result { get; set; } = "";
    }
}