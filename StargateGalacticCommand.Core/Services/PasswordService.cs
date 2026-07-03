using System;
using System.Security.Cryptography;

namespace StargateGalacticCommand.Core.Services
{
    public class PasswordService
    {
        private const int SaltSizeBytes = 16;
        private const int HashSizeBytes = 32;
        private const int Pbkdf2Iterations = 100000;

        public void CreateHash(string password, out string hash, out string salt)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8) throw new ArgumentException("Password must contain at least 8 characters.", "password");
            byte[] saltBytes = new byte[SaltSizeBytes];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(saltBytes);
            hash = Convert.ToBase64String(DeriveHash(password, saltBytes, Pbkdf2Iterations, HashAlgorithmName.SHA256));
            salt = Convert.ToBase64String(saltBytes);
        }

        public bool Verify(string password, string hash, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt)) return false;

            byte[] saltBytes;
            byte[] expectedHash;
            try
            {
                saltBytes = Convert.FromBase64String(salt);
                expectedHash = Convert.FromBase64String(hash);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] currentHash = DeriveHash(password, saltBytes, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            if (CryptographicOperations.FixedTimeEquals(currentHash, expectedHash)) return true;

            // Compatibility path for hashes created before the .NET 8 upgrade.
            byte[] legacyHash = DeriveHash(password, saltBytes, 10000, HashAlgorithmName.SHA1);
            return CryptographicOperations.FixedTimeEquals(legacyHash, expectedHash);
        }

        private static byte[] DeriveHash(string password, byte[] saltBytes, int iterations, HashAlgorithmName hashAlgorithm)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, hashAlgorithm))
                return pbkdf2.GetBytes(HashSizeBytes);
        }
    }
}
