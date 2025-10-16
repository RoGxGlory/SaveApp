// Provides password hashing and verification utilities for user authentication
namespace SaveAppCore;

public static class PasswordService
{
    // Size of the salt in bytes
    private const int SaltSize = 16;
    // Size of the hash in bytes
    private const int HashSize = 32;
    // Number of PBKDF2 iterations for hashing
    private const int Iterations = 100_000;

    /// <summary>
    /// Hashes a password using PBKDF2 with SHA256 and a random salt.
    /// Returns the hash and salt as base64 strings.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>Tuple of hash and salt, both base64 encoded.</returns>
    public static (string hashB64, string saltB64) HashPassword(string password)
    {
        byte[] salt = new byte[SaltSize];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
        var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, Iterations, System.Security.Cryptography.HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    /// <summary>
    /// Verifies a password against a stored hash and salt using PBKDF2 with SHA256.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="storedHashB64">The stored hash (base64).</param>
    /// <param name="storedSaltB64">The stored salt (base64).</param>
    /// <returns>True if the password matches, false otherwise.</returns>
    public static bool VerifyPassword(string password, string storedHashB64, string storedSaltB64)
    {
        byte[] salt = Convert.FromBase64String(storedSaltB64);
        byte[] storedHash = Convert.FromBase64String(storedHashB64);
        var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, Iterations, System.Security.Cryptography.HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(HashSize);
        // Use constant-time comparison to prevent timing attacks
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}
