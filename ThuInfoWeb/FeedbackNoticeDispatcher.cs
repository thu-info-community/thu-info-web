using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using ThuInfoWeb.Bots;

namespace ThuInfoWeb;

/// <summary>
/// Dispatches feedback notifications outside the HTTP request that created the feedback.
/// </summary>
public sealed partial class FeedbackNoticeDispatcher(
    FeedbackNoticeBot bot,
    ILogger<FeedbackNoticeDispatcher> logger) : BackgroundService
{
    private const int Capacity = 100;
    private readonly Channel<string> _notifications = Channel.CreateBounded<string>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    public bool TryEnqueue(string content)
    {
        if (_notifications.Writer.TryWrite(content))
            return true;

        LogQueueFull(logger);
        return false;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var content in _notifications.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await bot.PushNoticeAsync(content, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                LogSendFailed(logger, ex);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _notifications.Writer.TryComplete();
        return base.StopAsync(cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Feedback notification queue is full; notification was dropped.")]
    private static partial void LogQueueFull(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unable to send a queued feedback notification.")]
    private static partial void LogSendFailed(ILogger logger, Exception exception);
}
