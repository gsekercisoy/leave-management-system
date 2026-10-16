using System.Security.Cryptography;
using System.Text;

namespace LeaveApi.Services;

public static class PasswordHasher
{
    public static string GenerateSalt(int size = 16)
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(size));

    public static string Hash(string password, string salt)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salt + password));
        return Convert.ToBase64String(bytes);
    }

    public static bool Verify(string password, string salt, string hash)
        => CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(Hash(password, salt)),
            Convert.FromBase64String(hash));
}
