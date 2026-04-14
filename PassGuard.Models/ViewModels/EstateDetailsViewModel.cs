namespace PassGuard.Models.ViewModels
{
    public class EstateDetailsViewModel
    {
        public int EstateId { get; set; }
        public string EstateName { get; set; } = "";
        public int HomeCount { get; set; }
        public int VisitPassCount { get; set; }
        public int ActivePassCount { get; set; }
        public int CheckedInTodayCount { get; set; }
        public List<Home> Homes { get; set; } = new List<Home>();
    }
}
