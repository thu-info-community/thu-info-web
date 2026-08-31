using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ThuInfoWeb.Bots;

public sealed class FeedbackNoticeBot(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly bool _internalNetworkMode = configuration.GetValue("InternalNetworkMode", false);
    private readonly string _secret = configuration["FeishuBots:FeedbackNoticeBot:Secret"] ?? "";
    private readonly string _url = configuration["FeishuBots:FeedbackNoticeBot:Url"] ?? "";

    private string GetSign(long timestamp)
    {
        var str = $"{timestamp}\n{_secret}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(str));
        var code = hmac.ComputeHash([]);
        return Convert.ToBase64String(code);
    }

    public async Task PushNoticeAsync(string content, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient("feedback-notice");
        if (_internalNetworkMode)
        {
            using var internalContent = JsonContent.Create(new { Content = content, Secret = _secret });
            using var response = await httpClient.PostAsync(
                "https://stu.cs.tsinghua.edu.cn/thuinfo/botnotice",
                internalContent,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using var requestContent = JsonContent.Create(new
        {
            timestamp = timestamp.ToString(CultureInfo.InvariantCulture),
            sign = GetSign(timestamp),
            msg_type = "text",
            content = new { text = content }
        });
        using var feishuResponse = await httpClient.PostAsync(
            _url,
            requestContent,
            cancellationToken);
        feishuResponse.EnsureSuccessStatusCode();

        var json = await feishuResponse.Content.ReadAsStringAsync(cancellationToken);
        using var parsed = JsonDocument.Parse(json);
        if (!parsed.RootElement.TryGetProperty("StatusCode", out var code)
            && !parsed.RootElement.TryGetProperty("code", out code))
            throw new InvalidOperationException("Feedback notification response did not contain StatusCode.");
        if (code.GetInt32() != 0)
            throw new InvalidOperationException("Feedback notification failed.");
    }
}
