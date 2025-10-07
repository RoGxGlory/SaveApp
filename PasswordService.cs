namespace SaveApp;

public static class PasswordService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static (string hashB64, string saltB64) HashPassword(string password)
    {
        byte[] salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(SaltSize);
        var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, Iterations, System.Security.Cryptography.HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool VerifyPassword(string password, string storedHashB64, string storedSaltB64)
    {
        byte[] salt = Convert.FromBase64String(storedSaltB64);
        byte[] storedHash = Convert.FromBase64String(storedHashB64);
        var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, Iterations, System.Security.Cryptography.HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(HashSize);
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}

