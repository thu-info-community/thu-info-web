using System.Security.Cryptography;
using System.Text;

namespace ThuInfoWeb
{
    public static class Extension
    {
        public static string ToSHA256Hex(this string s)
        {
            var data = SHA256.HashData(Encoding.ASCII.GetBytes(s));
            string output = "";
            foreach (var b in data)
            {
                output += b.ToString("x").PadLeft(2, '0');
            }
            return output;
        }
    }
}
