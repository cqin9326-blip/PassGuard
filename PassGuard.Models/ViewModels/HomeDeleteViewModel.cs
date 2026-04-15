namespace PassGuard.Models.ViewModels
{
    public class HomeDeleteViewModel
    {
        public int HomeId { get; set; }
        public string EstateName { get; set; } = "";
        public string Address { get; set; } = "";
        public string HomeOwnerName { get; set; } = "";
        public int VisitPassCount { get; set; }
        public bool VisitPassesRemoved { get; set; }
    }
}
