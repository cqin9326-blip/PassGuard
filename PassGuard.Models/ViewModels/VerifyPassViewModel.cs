namespace PassGuard.Models.ViewModels
{
    public class VerifyPassViewModel
    {
        public string EnteredCode { get; set; } = "";
        public string Message { get; set; } = "";
        public string StatusLabel { get; set; } = "";
        public bool IsMatch { get; set; }
        public VisitPass? VisitPass { get; set; }
    }
}
