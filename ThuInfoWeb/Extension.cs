using System.Security.Cryptography;
using System.Text;

namespace ThuInfoWeb;

public static class Extension
{
    public static string ToSHA256Hex(this string s)
    {
        var data = SHA256.HashData(Encoding.ASCII.GetBytes(s));
        return data.Aggregate("", (current, b) => current + b.ToString("x").PadLeft(2, '0'));
    }
}
