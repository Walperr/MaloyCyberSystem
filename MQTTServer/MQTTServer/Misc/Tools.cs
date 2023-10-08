using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MQTTServer.Misc;

public static class Tools
{
    public static string ConvertToString(this ArraySegment<byte> bytes, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;

        return new string(encoding.GetChars(bytes.ToArray()));
    }

    public static string GetPasswordHash(this string password, string salt)
    {
        var bytes = salt.Select(c => (byte) c).ToArray();

        return Convert.ToBase64String(KeyDerivation.Pbkdf2(password, bytes, KeyDerivationPrf.HMACSHA256, 100000,
            256 / 8));
    }
}