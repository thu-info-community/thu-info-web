public class LoginAttemptService
{
    private static readonly Dictionary<string, (int Attempts, DateTime LastAttempt)> _loginAttempts = new();

    private const int MaxAttempts = 5; // Max allowed attempts
    private readonly TimeSpan _blockDuration = TimeSpan.FromMinutes(15); // Block duration

    public bool IsBlocked(string username)
    {
        if (_loginAttempts.TryGetValue(username, out var attemptData))
        {
            if (attemptData.Attempts >= MaxAttempts)
            {
                if (DateTime.UtcNow < attemptData.LastAttempt.Add(_blockDuration))
                {
                    return true; // Blocked
                }
                else
                {
                    // Reset attempts after block duration
                    _loginAttempts.Remove(username);
                }
            }
        }
        return false;
    }

    public void RecordAttempt(string username)
    {
        if (_loginAttempts.ContainsKey(username))
        {
            _loginAttempts[username] = (_loginAttempts[username].Attempts + 1, DateTime.UtcNow);
        }
        else
        {
            _loginAttempts[username] = (1, DateTime.UtcNow);
        }
    }
}
