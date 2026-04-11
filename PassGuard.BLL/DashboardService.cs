using PassGuard.DAL;
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
                ActivePasses = _visitPassRepository.CountActivePasses(),
                TodayCheckInCount = _gateCheckInRepository.GetTodayCount(),
                TodayVisitors = visitPasses
                    .Where(v => v.CreatedAt.Date == today || v.GateCheckIns.Any(g => g.CheckInTime.Date == today))
                    .Select(v => new DashboardVisitorRowViewModel
                    {
                        VisitorName = v.VisitorName,
                        HomeAddress = v.Home.Address,
                        EstateName = v.Home.Estate.EstateName,
                        CreatedAt = v.CreatedAt,
                        ExpiresAt = v.ExpiresAt,
                        Status = v.Status,
                        Result = v.GateCheckIns
                            .OrderByDescending(g => g.CheckInTime)
                            .FirstOrDefault()?.Result ?? "N/A"
                    })
                    .ToList()
            };

            return vm;
        }
    }
}