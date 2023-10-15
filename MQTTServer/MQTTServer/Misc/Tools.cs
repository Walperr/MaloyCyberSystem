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
    
    public static int QuickSearch<T>(this T[] arr, Predicate<T> predicate)
    {
        if (arr.Length == 0)
            return -1;

        var startIndex = 0;
        var endIndex = arr.Length - 1;

        while (startIndex <= endIndex && !predicate(arr[startIndex]))
        {
            var middle = (startIndex + endIndex) / 2;
            if (predicate(arr[middle]))
                endIndex = middle;
            else
                startIndex = middle + 1;
        }

        if (endIndex <= 0)
            return -1;

        if (startIndex >= arr.Length)
            return arr.Length;

        return startIndex;
    }
}