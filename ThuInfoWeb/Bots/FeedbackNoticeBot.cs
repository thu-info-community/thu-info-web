using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ThuInfoWeb.Bots;

public class FeedbackNoticeBot(IConfiguration configuration)
{
    private readonly HttpClient _httpClient = new();
    private readonly bool _internalNetworkMode = bool.Parse(configuration["InternalNetworkMode"] ?? "false");
    private readonly string _secret = configuration["FeishuBots:FeedbackNoticeBot:Secret"] ?? "";
    private readonly string _url = configuration["FeishuBots:FeedbackNoticeBot:Url"] ?? "";

    private string GetSign(long timestamp)
    {
        var str = $"{timestamp}\n{_secret}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(str));
        var code = hmac.ComputeHash([]);
        var sign = Convert.ToBase64String(code);
        return sign;
    }

    public async Task PushNoticeAsync(string content)
    {
        if (_internalNetworkMode)
        {
            var resp = await _httpClient.PostAsync("https://stu.cs.tsinghua.edu.cn/thuinfo/botnotice",
                JsonContent.Create(new { Content = content, Secret = _secret }));
            resp.EnsureSuccessStatusCode();
        }
        else
        {
            var ts = DateTimeOffset.Now.ToUnixTimeSeconds();
            var resp = await _httpClient.PostAsync(_url,
                JsonContent.Create(new
                {
                    timestamp = ts.ToString(),
                    sign = GetSign(ts),
                    msg_type = "text",
                    content = new { text = content }
                }));
            var json = await resp.Content.ReadAsStringAsync();
            var parsed = JsonDocument.Parse(json);
            if (!parsed.RootElement.TryGetProperty("StatusCode", out var code))
                throw new Exception("Send error");
            if (code.GetInt32() != 0)
                throw new Exception("Send error");
        }
    }
}
