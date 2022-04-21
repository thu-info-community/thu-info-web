using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ThuInfoWeb.Bots
{
    public class FeedbackNoticeBot
    {
        private readonly string _url;
        private readonly string _secret;
        private readonly HttpClient _httpClient;
        public FeedbackNoticeBot(string url,string secret)
        {
            this._url = url;
            this._secret = secret;
            this._httpClient = new HttpClient();
        }
        private string GetSign(long timestamp)
        {
            var str = $"{timestamp}\n{_secret}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(str));
            var code = hmac.ComputeHash(new byte[0]);
            var sign = Convert.ToBase64String(code);
            return sign;
        }
        public async Task PushNoticeAsync(string content)
        {
            var ts = DateTimeOffset.Now.ToUnixTimeSeconds();
            var resp = await _httpClient.PostAsync(_url, JsonContent.Create(new
            {
                timestamp = ts.ToString(),
                sign = GetSign(ts),
                msg_type = "text",
                content = new
                {
                    text = content
                }
            }));
            var json =await resp.Content.ReadAsStringAsync();
            var parsed = JsonDocument.Parse(json);
            if (!parsed.RootElement.TryGetProperty("StatusCode", out var code))
                throw new Exception("Send error");
            else if (code.GetInt32() != 0)
                throw new Exception("Send error");
            else
                return;
        }
    }
}
