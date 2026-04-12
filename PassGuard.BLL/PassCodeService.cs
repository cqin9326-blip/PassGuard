using System.Security.Cryptography;
using System.Text;
using PassGuard.Models;

namespace PassGuard.BLL
{
    public class PassCodeService
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public string GeneratePlainCode(int length = 8)
        {
            StringBuilder builder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                builder.Append(Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]);
            }

            return builder.ToString();
        }

        public string HashCode(string plainCode)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                plainCode,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool VerifyCode(string plainCode, string storedHash)
        {
            try
            {
                string[] parts = storedHash.Split('.');

                if (parts.Length != 3 || !int.TryParse(parts[0], out int iterations))
                {
                    return false;
                }

                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] expectedHash = Convert.FromBase64String(parts[2]);

                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    plainCode,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public string CalculateStatus(VisitPass visitPass, DateTime? now = null)
        {
            DateTime current = now ?? DateTime.Now;

            if (string.Equals(visitPass.Status, PassStatuses.Revoked, StringComparison.OrdinalIgnoreCase))
            {
                return PassStatuses.Revoked;
            }

            if (visitPass.GateCheckIn != null && string.Equals(visitPass.GateCheckIn.Result, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                return PassStatuses.Used;
            }

            if (visitPass.ExpiresAt <= current)
            {
                return PassStatuses.Expired;
            }

            return PassStatuses.Active;
        }
    }
}
