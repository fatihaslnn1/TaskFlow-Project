using System.Security.Cryptography;
using System.Text;

namespace TaskFlow.Infrastructure.Services;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}