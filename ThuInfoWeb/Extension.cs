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

    /// <summary>
    ///   Compares this version with another version.
    /// </summary>
    /// <param name="v">Current version number</param>
    /// <param name="other">Other version number</param>
    /// <returns>true if the current version number is greater than the other one</returns>
    /// <exception cref="ArgumentException">if either part is not a valid version number</exception>
    public static bool VersionGreaterThan(this string v, string other)
    {
        if (!v.IsValidVersionNumber() || !other.IsValidVersionNumber())
            throw new ArgumentException("Invalid version number format.");

        var vParts = v.Split('.').Select(int.Parse).ToArray();
        var oParts = other.Split('.').Select(int.Parse).ToArray();

        for (var i = 0; i < 3; i++)
        {
            var vPart = i < vParts.Length ? vParts[i] : 0;
            var oPart = i < oParts.Length ? oParts[i] : 0;

            if (vPart > oPart) return true;
            if (vPart < oPart) return false;
        }

        return false;
    }
}
