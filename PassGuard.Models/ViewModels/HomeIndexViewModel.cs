namespace PassGuard.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public int? SelectedEstateId { get; set; }
        public string SelectedEstateName { get; set; } = "";
        public List<HomeEstateOptionViewModel> Estates { get; set; } = new List<HomeEstateOptionViewModel>();
        public List<Home> Homes { get; set; } = new List<Home>();
    }

    public class HomeEstateOptionViewModel
    {
        public int EstateId { get; set; }
        public string EstateName { get; set; } = "";
        public bool Selected { get; set; }
    }
}
