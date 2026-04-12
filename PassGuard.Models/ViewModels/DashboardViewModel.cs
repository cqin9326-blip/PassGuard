namespace PassGuard.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEstates { get; set; }
        public int TotalHomes { get; set; }
        public int TotalUsers { get; set; }
        public int TotalVisitors { get; set; }
        public int ActivePasses { get; set; }
        public int UsedPasses { get; set; }
        public int ExpiredPasses { get; set; }
        public int RevokedPasses { get; set; }
        public int TodayCheckInCount { get; set; }
        public List<DashboardVisitorRowViewModel> TodayVisitors { get; set; } = new List<DashboardVisitorRowViewModel>();
        public List<DashboardActivityViewModel> RecentActivity { get; set; } = new List<DashboardActivityViewModel>();
        public List<AuditLogListItemViewModel> RecentLogs { get; set; } = new List<AuditLogListItemViewModel>();
    }

    public class DashboardVisitorRowViewModel
    {
        public string VisitorName { get; set; } = "";
        public string HomeAddress { get; set; } = "";
        public string EstateName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string ExpiresDisplay { get; set; } = "";
        public string Status { get; set; } = "";
        public string Result { get; set; } = "";
    }

    public class DashboardActivityViewModel
    {
        public string Title { get; set; } = "";
        public string Detail { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
