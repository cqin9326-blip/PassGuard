namespace PassGuard.Models
{
    public class Estate
    {
        public int EstateId { get; set; }
        public string EstateName { get; set; } = "";

        public List<Home> Homes { get; set; } = new List<Home>();
    }
}