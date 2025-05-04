using System;
using System.Security.Cryptography;

namespace Blog_website.Services
{
    public static class PasswordHasher
    {
        // Generate a salt for password hashing
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // Hash a password with the given salt
        public static string HashPassword(string password, string salt)
        {
            // Use BCrypt.Net-Next for secure password hashing
            return BCrypt.Net.BCrypt.HashPassword(password + salt);
        }

        // Verify a password against a stored hash
        public static bool VerifyPassword(string password, string salt, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password + salt, hash);
        }
    }
}
