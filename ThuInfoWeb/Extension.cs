using System.Security.Cryptography;
using System.Text;

namespace ThuInfoWeb
{
    public static class Extension
    {
        public static string ToMd5Hex(this string s)
        {
            var data = MD5.HashData(Encoding.ASCII.GetBytes(s));
            string output = "";
            foreach (var b in data)
            {
                output += b.ToString("x");
            }
            return output;
        }
    }
}
