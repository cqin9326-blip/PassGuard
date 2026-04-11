namespace PassGuard.Models
{
    public static class PassStatuses
    {
        public const string Active = "Active";
        public const string Used = "Used";
        public const string Expired = "Expired";
        public const string Revoked = "Revoked";

        public static readonly string[] Allowed = { Active, Used, Expired, Revoked };
    }
}
