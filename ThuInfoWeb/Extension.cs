using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ThuInfoWeb;

public static partial class Extension
{
    [GeneratedRegex(@"^\d+\.\d+\.\d+$")]
    private static partial Regex VersionRegex();

    public static string ToSHA256Hex(this string s)
    {
        var data = SHA256.HashData(Encoding.ASCII.GetBytes(s));
        return data.Aggregate("", (current, b) => current + b.ToString("x").PadLeft(2, '0'));
    }

    public static bool IsValidVersionNumber(this string s)
    {
        return VersionRegex().IsMatch(s);
    }
}
