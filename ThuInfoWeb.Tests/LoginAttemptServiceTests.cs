using Xunit;

namespace ThuInfoWeb.Tests;

public class LoginAttemptServiceTests
{
    [Fact]
    public void AttemptsBlockAndExpireUsingInjectedTime()
    {
        var clock = new TestTimeProvider(DateTimeOffset.UnixEpoch);
        var service = new LoginAttemptService(clock);

        for (var i = 0; i < 5; i++)
            service.RecordAttempt("Alice");

        Assert.True(service.IsBlocked("alice"));
        clock.Advance(TimeSpan.FromMinutes(16));
        Assert.False(service.IsBlocked("alice"));
    }

    [Fact]
    public void SuccessfulLoginCanClearAttempts()
    {
        var service = new LoginAttemptService();
        service.RecordAttempt("alice");
        service.ClearAttempts("alice");

        Assert.False(service.IsBlocked("alice"));
    }

    private sealed class TestTimeProvider(DateTimeOffset current) : TimeProvider
    {
        private DateTimeOffset _current = current;

        public override DateTimeOffset GetUtcNow() => _current;

        public void Advance(TimeSpan amount) => _current += amount;
    }
}
