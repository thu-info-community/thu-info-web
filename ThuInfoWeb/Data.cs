using FreeSql;
using FreeSql.Internal;
using ThuInfoWeb.DBModels;
using Version = ThuInfoWeb.DBModels.Version;

namespace ThuInfoWeb;

public class Data
{
    private readonly IFreeSql _fsql;

    /// <summary>
    /// </summary>
    /// <param name="connectionString">the connection string</param>
    /// <param name="isDevelopment">
    ///     if env is development, use local sqlite database instead of remote postgresql. The DB file
    ///     will be created automatically.
    /// </param>
    public Data(string connectionString, bool isDevelopment)
    {
        if (isDevelopment)
            _fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "Data Source=test.db")
                .UseAutoSyncStructure(true)
                .UseNameConvert(NameConvertType.ToLower)
                .UseMonitorCommand(x => Console.WriteLine(x.CommandText))
                .Build();
        else
            _fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.PostgreSQL, connectionString)
                .UseAutoSyncStructure(true)
                .UseNameConvert(NameConvertType.ToLower)
                .Build();
        // Check if there is a misc record in database, create one if not exist.
        if (!_fsql.Select<Misc>().Any())
            _fsql.Insert(new Misc()).ExecuteAffrows();
    }

    public async Task<Misc?> GetMiscAsync()
    {
        return await _fsql.Select<Misc>().FirstAsync();
    }

    public async Task<int> UpdateMiscAsync(Misc misc)
    {
        return await _fsql.Update<Misc>().SetSource(misc).ExecuteAffrowsAsync();
    }

    public async Task<User?> GetUserAsync(string name)
    {
        return await _fsql.Select<User>().Where(x => x.Name == name).ToOneAsync();
    }

    public async Task<bool> CheckUserAsync(string name)
    {
        return await _fsql.Select<User>().AnyAsync(x => x.Name == name);
    }

    public async Task<int> CreateUserAsync(User user)
    {
        return await _fsql.Select<User>().AnyAsync(x => x.Name == user.Name)
            ? 0
            : await _fsql.Insert(user).ExecuteAffrowsAsync();
    }

    public async Task<int> ChangeUserPasswordAsync(string name, string passwordHash)
    {
        return await _fsql.Update<User>().Where(x => x.Name == name).Set(x => x.PasswordHash, passwordHash)
            .ExecuteAffrowsAsync();
    }

    public async Task<Announce?> GetAnnounceAsync(int? id = null)
    {
        return id is null
            ? await _fsql.Select<Announce>().OrderByDescending(x => x.Id).FirstAsync()
            : await _fsql.Select<Announce>().Where(x => x.Id == id).ToOneAsync();
    }

    public async Task<Announce?> GetActiveAnnounceAsync(int? id = null)
    {
        return id is null
            ? await _fsql.Select<Announce>().OrderByDescending(x => x.Id).Where(x => x.IsActive).FirstAsync()
            : await _fsql.Select<Announce>().Where(x => x.Id == id && x.IsActive).ToOneAsync();
    }

    public async Task<List<Announce>> GetAnnouncesAsync(int page, int pageSize)
    {
        return await _fsql.Select<Announce>().OrderByDescending(x => x.Id).Page(page, pageSize).ToListAsync();
    }

    public async Task<List<Announce>> GetActiveAnnouncesAsync(int page, int pageSize, string version)
    {
        var l = await _fsql.Select<Announce>()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        // The exact match can't be performed in the SQL query, so we have to filter it in memory.
        var filtered = l.Where(x => x.VisibleExact.Split(',').Any(v => v == version) || !version.VersionGreaterThan(x.VisibleNotAfter));

        return [.. filtered.Skip((page - 1) * pageSize).Take(pageSize)];
    }

    public async Task<int> CreateAnnounceAsync(Announce a)
    {
        return await _fsql.Insert(a).ExecuteAffrowsAsync();
    }

    public async Task<int> UpdateAnnounceStatusAsync(int id, bool toActive)
    {
        return await _fsql.Update<Announce>().Where(x => x.Id == id).Set(x => x.IsActive, toActive)
            .ExecuteAffrowsAsync();
    }

    public async Task<int> DeleteAnnounceAsync(int id)
    {
        return await _fsql.Delete<Announce>().Where(x => x.Id == id).ExecuteAffrowsAsync();
    }

    public async Task<int> CreateFeedbackAsync(Feedback feedback)
    {
        return await _fsql.Insert(feedback).ExecuteAffrowsAsync();
    }

    public async Task<Feedback?> GetFeedbackAsync(int? id = null)
    {
        return id is null
            ? await _fsql.Select<Feedback>().OrderByDescending(x => x.Id).FirstAsync()
            : await _fsql.Select<Feedback>().Where(x => x.Id == id).ToOneAsync();
    }

    public async Task<List<Feedback>> GetFeedbacksAsync(int page, int pageSize)
    {
        return await _fsql.Select<Feedback>().OrderByDescending(x => x.Id).Page(page, pageSize).ToListAsync();
    }

    public async Task<List<Feedback>> GetAllRepliedFeedbacksAsync()
    {
        return await _fsql.Select<Feedback>().Where(x => !string.IsNullOrEmpty(x.Reply))
            .OrderByDescending(x => x.RepliedTime).ToListAsync();
    }

    public async Task<int> DeleteFeedbackAsync(int id)
    {
        return await _fsql.Delete<Feedback>().Where(x => x.Id == id).ExecuteAffrowsAsync();
    }

    public async Task<int> ReplyFeedbackAsync(int id, string reply, string replier)
    {
        return await _fsql.Update<Feedback>().Where(x => x.Id == id).Set(x => x.Reply, reply)
            .Set(x => x.ReplierName, replier).Set(x => x.RepliedTime, DateTime.Now).ExecuteAffrowsAsync();
    }

    public async Task<List<Socket>> GetSocketsAsync(int sectionId)
    {
        return await _fsql.Select<Socket>().Where(x => x.SectionId == sectionId).ToListAsync();
    }

    public async Task<int> UpdateSocketAsync(int seatId, bool isAvailable)
    {
        return await _fsql.Update<Socket>().Where(x => x.SeatId == seatId).Set(x => x.Status,
            isAvailable ? Socket.SocketStatus.Available : Socket.SocketStatus.Unavailable).ExecuteAffrowsAsync();
    }

    public async Task<int> CreateVersionAsync(Version version)
    {
        return await _fsql.Insert(version).ExecuteAffrowsAsync();
    }

    public async Task<Version?> GetVersionAsync(bool isAndroid)
    {
        return await _fsql.Select<Version>().Where(x => x.IsAndroid == isAndroid).OrderByDescending(x => x.CreatedTime)
            .FirstAsync();
    }

    public async Task<int> CreateHttpRequestLogAsync(Request r)
    {
        return await _fsql.Insert(r).ExecuteAffrowsAsync();
    }

    public async Task<int> CreateUsageAsync(Usage u)
    {
        return await _fsql.Insert(u).ExecuteAffrowsAsync();
    }

    public async Task<Dictionary<Usage.FunctionType, int>> GetUsageAsync()
    {
        return await _fsql.Select<Usage>().GroupBy(x => x.Function).ToDictionaryAsync(x => x.Count());
    }

    public async Task<int> CreateStartupAsync(Startup s)
    {
        return await _fsql.Insert(s).ExecuteAffrowsAsync();
    }

    public async Task<List<JieliWasher>> GetJieliWashersAsync(string? building = null)
    {
        var query = _fsql.Select<JieliWasher>();
        if (!string.IsNullOrWhiteSpace(building))
            query = query.Where(x => x.Building == building);
        return await query.OrderBy(x => x.Building).ToListAsync();
    }

    public async Task<int> CreateJieliWasherAsync(JieliWasher washer)
    {
        return await _fsql.Insert(washer).ExecuteAffrowsAsync();
    }

    public async Task<int> UpdateJieliWasherAsync(string id, string building, string name)
    {
        return await _fsql.Update<JieliWasher>()
            .Where(x => x.Id == id)
            .Set(x => x.Building, building)
            .Set(x => x.Name, name)
            .ExecuteAffrowsAsync();
    }

    public async Task<int> DeleteJieliWasherAsync(string id)
    {
        return await _fsql.Delete<JieliWasher>().Where(x => x.Id == id).ExecuteAffrowsAsync();
    }

    public async Task<Dictionary<string, int>> GetStartupDataAsync()
    {
        return await _fsql.Select<Startup>().GroupBy(x => x.CreatedTime.ToString("yyyy MM"))
            .OrderBy(x => x.Key)
            .ToDictionaryAsync(x => x.Count());
    }

    public async Task<Dictionary<string, double>> GetWeeklyAverageDailyActiveUsersAsync()
    {
        var dailyActiveUsers = await GetDailyActiveUsersAsync();
        if (dailyActiveUsers.Count == 0)
            return [];

        var startDate = dailyActiveUsers.Keys.Min();
        var endDate = dailyActiveUsers.Keys.Max();
        var weeklyBuckets = new SortedDictionary<DateTime, List<int>>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var weekStart = GetWeekStart(date);
            if (!weeklyBuckets.TryGetValue(weekStart, out var bucket))
            {
                bucket = [];
                weeklyBuckets[weekStart] = bucket;
            }

            bucket.Add(dailyActiveUsers.GetValueOrDefault(date, 0));
        }

        return weeklyBuckets.ToDictionary(
            x => x.Key.ToString("yyyy-MM-dd"),
            x => Math.Round(x.Value.Average(), 2));
    }

    public async Task<Dictionary<string, double>> GetWeekdayAverageActiveUsersAsync()
    {
        var dailyActiveUsers = await GetDailyActiveUsersAsync();
        if (dailyActiveUsers.Count == 0)
            return [];

        var startDate = dailyActiveUsers.Keys.Min();
        var endDate = dailyActiveUsers.Keys.Max();
        var weekdayBuckets = new Dictionary<DayOfWeek, List<int>>
        {
            [DayOfWeek.Monday] = [],
            [DayOfWeek.Tuesday] = [],
            [DayOfWeek.Wednesday] = [],
            [DayOfWeek.Thursday] = [],
            [DayOfWeek.Friday] = [],
            [DayOfWeek.Saturday] = [],
            [DayOfWeek.Sunday] = []
        };

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
            weekdayBuckets[date.DayOfWeek].Add(dailyActiveUsers.GetValueOrDefault(date, 0));

        var orderedDays = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
            DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };
        return orderedDays.ToDictionary(
            day => day.ToString(),
            day => Math.Round(weekdayBuckets[day].Average(), 2));
    }

    private async Task<Dictionary<DateTime, int>> GetDailyActiveUsersAsync()
    {
        var startups = await _fsql.Select<Startup>()
            .Where(x => x.Uuid != null)
            .ToListAsync(x => new { x.CreatedTime, x.Uuid });

        return startups
            .GroupBy(x => x.CreatedTime.Date)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.Uuid!.Value).Distinct().Count());
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7; // Monday = 0
        return date.AddDays(-offset).Date;
    }
#if DEBUG
    public async Task GenStartupDataAsync()
    {
        var s1 = new Startup { CreatedTime = DateTime.Now - TimeSpan.FromDays(30) };
        for (var i = 0; i < 10; i++)
            await _fsql.Insert(s1).ExecuteAffrowsAsync();

        var s2 = new Startup { CreatedTime = DateTime.Now - TimeSpan.FromDays(60) };
        for (var i = 0; i < 20; i++)
            await _fsql.Insert(s2).ExecuteAffrowsAsync();

        var s3 = new Startup { CreatedTime = DateTime.Now - TimeSpan.FromDays(90) };
        for (var i = 0; i < 30; i++)
            await _fsql.Insert(s3).ExecuteAffrowsAsync();
    }
#endif
}
