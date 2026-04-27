using System;
using System.Security.Cryptography;

namespace LTW_CK_QuangBaVietNam.Helpers
{
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password is required.", nameof(password));
            }

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] key;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                key = pbkdf2.GetBytes(KeySize);
            }

            return string.Format("{0}${1}${2}${3}", Prefix, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(key));
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            if (!IsHashedFormat(storedHash))
            {
                return storedHash == inputPassword;
            }

            string[] parts = storedHash.Split('$');
            if (parts.Length != 4)
            {
                return false;
            }

            int iterations;
            if (!int.TryParse(parts[1], out iterations))
            {
                return false;
            }

            byte[] salt;
            byte[] expectedKey;

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedKey = Convert.FromBase64String(parts[3]);
            }
            catch
            {
                return false;
            }

            byte[] actualKey;
            using (var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, iterations, HashAlgorithmName.SHA256))
            {
                actualKey = pbkdf2.GetBytes(expectedKey.Length);
            }

            return FixedTimeEquals(actualKey, expectedKey);
        }

        public static bool IsHashedFormat(string storedHash)
        {
            return !string.IsNullOrEmpty(storedHash) && storedHash.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
