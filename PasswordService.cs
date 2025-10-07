// Service statique pour le hashage et la vérification sécurisée des mots de passe utilisateurs

using System.Security.Cryptography;

namespace SaveApp.SaveApp;

public static class PasswordService
{
    // Taille du sel utilisé pour le hash (en octets)
    private const int SaltSize = 16;        // 128 bits
    // Taille du hash généré (en octets)
    private const int HashSize = 32;        // 256 bits
    // Nombre d'itérations pour PBKDF2 (plus = plus sécurisé mais plus lent)
    private const int Iterations = 100_000; // nombre d'itérations PBKDF2

    // Génère le hash et le sel à partir du mot de passe fourni
    public static (string hashB64, string saltB64) HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    // Vérifie si un mot de passe correspond au hash et au sel stockés
    public static bool VerifyPassword(string password, string storedHashB64, string storedSaltB64)
    {
        byte[] salt = Convert.FromBase64String(storedSaltB64);
        byte[] storedHash = Convert.FromBase64String(storedHashB64);

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(HashSize);

        // Comparaison sécurisée pour éviter les attaques par timing
        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}