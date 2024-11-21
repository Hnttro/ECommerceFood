using System.Security.Cryptography;
using System.Text;

namespace ECommerceFood.Services.PaswordH
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var hashedProvidedPassword = HashPassword(providedPassword);
            return hashedPassword == hashedProvidedPassword;
        }
    }
}
