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
        private readonly bool _internalNetworkMode;

        public FeedbackNoticeBot(IConfiguration configuration)
        {
            this._url = configuration["FeishuBots:FeedbackNoticeBot:Url"];
            this._secret = configuration["FeishuBots:FeedbackNoticeBot:Secret"];
            this._internalNetworkMode = bool.Parse(configuration["InternalNetworkMode"]);
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
            if (_internalNetworkMode)
            {
                var resp = await _httpClient.PostAsync("https://stu.cs.tsinghua.edu.cn/thuinfo/botnotice", JsonContent.Create(new
                {
                    Content = content,
                    Secret = _secret
                }));
                resp.EnsureSuccessStatusCode();
            }
            else
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
                var json = await resp.Content.ReadAsStringAsync();
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
}