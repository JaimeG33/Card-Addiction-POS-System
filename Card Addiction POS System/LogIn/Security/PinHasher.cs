using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace Card_Addiction_POS_System.LogIn.Security
{
    public static class PinHasher
    {
        public const int SaltSize = 16;       // 16 bytes
        public const int HashSize = 32;       // 32 bytes output
        public const int DefaultIterations = 150_000;

        public static (byte[] salt, byte[] hash, int iterations) HashPin(string pin, int? iterations = null)
        {
            if (string.IsNullOrWhiteSpace(pin))
                throw new ArgumentException("PIN cannot be empty.");

            int iters = iterations ?? DefaultIterations;

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: pin,
                salt: salt,
                iterations: iters,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: HashSize
            );

            return (salt, hash, iters);
        }

        public static bool VerifyPin(string pin, byte[] salt, byte[] expectedHash, int iterations)
        {
            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password: pin,
                salt: salt,
                iterations: iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: expectedHash.Length
            );

            // Constant-time compare to avoid timing attacks
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
