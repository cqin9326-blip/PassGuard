using PassGuard.DAL;
using PassGuard.Models;
using PassGuard.Models.ViewModels;

namespace PassGuard.BLL
{
    public class DashboardService
    {
        private readonly EstateRepository _estateRepository;
        private readonly HomeRepository _homeRepository;
        private readonly VisitPassRepository _visitPassRepository;
        private readonly GateCheckInRepository _gateCheckInRepository;

        public DashboardService(
            EstateRepository estateRepository,
            HomeRepository homeRepository,
            VisitPassRepository visitPassRepository,
            GateCheckInRepository gateCheckInRepository)
        {
            _estateRepository = estateRepository;
            _homeRepository = homeRepository;
            _visitPassRepository = visitPassRepository;
            _gateCheckInRepository = gateCheckInRepository;
        }

        public DashboardViewModel GetDashboard()
        {
            DateTime today = DateTime.Today;

            var visitPasses = _visitPassRepository.GetAllWithDetails();

            DashboardViewModel vm = new DashboardViewModel
            {
                TotalEstates = _estateRepository.Count(),
                TotalHomes = _homeRepository.Count(),
                TotalVisitors = _visitPassRepository.CountDistinctVisitors(),
                ActivePasses = visitPasses.Count(v => v.Status == PassStatuses.Active),
                UsedPasses = visitPasses.Count(v => v.Status == PassStatuses.Used),
                ExpiredPasses = visitPasses.Count(v => v.Status == PassStatuses.Expired),
                RevokedPasses = visitPasses.Count(v => v.Status == PassStatuses.Revoked),
                TodayCheckInCount = _gateCheckInRepository.GetTodayCount(),
                TodayVisitors = visitPasses
                    .Where(v => v.CreatedAt.Date == today || v.GateCheckIn != null && v.GateCheckIn.CheckInTime.Date == today)
                    .Select(v => new DashboardVisitorRowViewModel
                    {
                        VisitorName = v.Visitor.FullName,
                        HomeAddress = v.Home.Address,
                        EstateName = v.Home.Estate.EstateName,
                        CreatedAt = v.CreatedAt,
                        ExpiresAt = v.ExpiresAt,
                        Status = v.Status,
                        Result = v.GateCheckIn?.Result ?? "N/A"
                    })
                    .OrderByDescending(v => v.CreatedAt)
                    .ToList(),
                RecentActivity = visitPasses
                    .SelectMany(v =>
                    {
                        List<DashboardActivityViewModel> items = new List<DashboardActivityViewModel>
                        {
                            new DashboardActivityViewModel
                            {
                                Title = "Pass Created",
                                Detail = $"{v.Visitor.FullName} for {v.Home.Address} ({v.Status})",
                                Timestamp = v.CreatedAt
                            }
                        };

                        if (v.GateCheckIn != null)
                        {
                            items.Add(new DashboardActivityViewModel
                            {
                                Title = $"Check-In {v.GateCheckIn.Result}",
                                Detail = $"{v.Visitor.FullName} at {v.Home.Address}",
                                Timestamp = v.GateCheckIn.CheckInTime
                            });
                        }

                        return items;
                    })
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .ToList()
            };

            return vm;
        }

        public HomeOwnerDashboardViewModel GetHomeOwnerDashboard(string ownerUserId)
        {
            List<VisitPass> visitPasses = _visitPassRepository.GetByCreatedByUserId(ownerUserId);

            return new HomeOwnerDashboardViewModel
            {
                ActivePassCount = visitPasses.Count(v => v.Status == PassStatuses.Active),
                PastPassCount = visitPasses.Count(v => v.Status == PassStatuses.Used || v.Status == PassStatuses.Expired),
                RevokedPassCount = visitPasses.Count(v => v.Status == PassStatuses.Revoked),
                ActivePasses = visitPasses
                    .Where(v => v.Status == PassStatuses.Active)
                    .OrderBy(v => v.ExpiresAt)
                    .ToList(),
                VisitHistory = visitPasses
                    .Where(v => v.GateCheckIn != null || v.Status != PassStatuses.Active)
                    .OrderByDescending(v => v.GateCheckIn?.CheckInTime ?? v.CreatedAt)
                    .ToList()
            };
        }

        public SecurityPanelViewModel GetSecurityPanel()
        {
            DateTime today = DateTime.Today;
            List<VisitPass> visitPasses = _visitPassRepository.GetAllWithDetails();

            List<VisitPass> expectedVisitors = visitPasses
                .Where(v =>
                    v.Status == PassStatuses.Active &&
                    v.ExpiresAt.Date >= today &&
                    v.GateCheckIn == null)
                .OrderBy(v => v.ExpiresAt)
                .ToList();

            List<VisitPass> recentChecks = visitPasses
                .Where(v => v.GateCheckIn != null && v.GateCheckIn.CheckInTime.Date == today)
                .OrderByDescending(v => v.GateCheckIn!.CheckInTime)
                .ToList();

            return new SecurityPanelViewModel
            {
                ExpectedVisitorCount = expectedVisitors.Count,
                CheckedInTodayCount = recentChecks.Count,
                ExpectedVisitors = expectedVisitors,
                RecentChecks = recentChecks
            };
        }
    }
}
