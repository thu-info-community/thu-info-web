using System.Collections.Concurrent;

namespace ThuInfoWeb;

public sealed class LoginAttemptService(TimeProvider? timeProvider = null)
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(15);
    private readonly ConcurrentDictionary<string, Attempt> _loginAttempts = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public bool IsBlocked(string username)
    {
        if (!_loginAttempts.TryGetValue(username, out var attempt) || attempt.Attempts < MaxAttempts)
            return false;

        if (_timeProvider.GetUtcNow() < attempt.LastAttempt.Add(BlockDuration))
            return true;

        ((ICollection<KeyValuePair<string, Attempt>>)_loginAttempts)
            .Remove(new KeyValuePair<string, Attempt>(username, attempt));
        return false;
    }

    public void RecordAttempt(string username)
    {
        var now = _timeProvider.GetUtcNow();
        _loginAttempts.AddOrUpdate(
            username,
            _ => new Attempt(1, now),
            (_, attempt) => new Attempt(attempt.Attempts + 1, now));
    }

    public void ClearAttempts(string username)
    {
        _loginAttempts.TryRemove(username, out _);
    }

    private readonly record struct Attempt(int Attempts, DateTimeOffset LastAttempt);
}
