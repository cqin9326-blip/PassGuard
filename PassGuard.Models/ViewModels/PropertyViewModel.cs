namespace PassGuard.Models.ViewModels
{
    public class PropertyViewModel
    {
        public int HomeId { get; set; }

        public string EstateName { get; set; } = "";
        public string OwnerUserId { get; set; } = "";
        public string Address { get; set; } = "";

        public string VisitorFullName { get; set; } = "";
        public string VisitorPhone { get; set; } = "";
    }
}
