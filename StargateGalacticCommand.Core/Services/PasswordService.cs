using System;
using System.Security.Cryptography;

namespace StargateGalacticCommand.Core.Services
{
    public class PasswordService
    {
        public void CreateHash(string password, out string hash, out string salt)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8) throw new ArgumentException("Password must contain at least 8 characters.", "password");
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(saltBytes);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000))
            {
                hash = Convert.ToBase64String(pbkdf2.GetBytes(32));
                salt = Convert.ToBase64String(saltBytes);
            }
        }
        public bool Verify(string password, string hash, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt)) return false;
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000))
                return Convert.ToBase64String(pbkdf2.GetBytes(32)) == hash;
        }
    }
}
