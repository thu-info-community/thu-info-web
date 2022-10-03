using ThuInfoWeb.DBModels;
using Version = ThuInfoWeb.DBModels.Version;

namespace ThuInfoWeb
{
    public class Data
    {
        private readonly IFreeSql _fsql;

        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionString">the connection string</param>
        /// <param name="isDevelopment">if env is development, use local sqlite database instead of remote postgresql. The DB file will be created automatically.</param>
        public Data(string connectionString, bool isDevelopment)
        {
            if (isDevelopment)
                _fsql = new FreeSql.FreeSqlBuilder()
                    .UseConnectionString(FreeSql.DataType.Sqlite, "Data Source=test.db")
                    .UseAutoSyncStructure(true)
                    .UseNameConvert(FreeSql.Internal.NameConvertType.ToLower)
                    .UseMonitorCommand(x => Console.WriteLine(x.CommandText))
                    .Build();
            else
                _fsql = new FreeSql.FreeSqlBuilder()
                     .UseConnectionString(FreeSql.DataType.PostgreSQL, connectionString)
                     .UseAutoSyncStructure(true)
                     .UseNameConvert(FreeSql.Internal.NameConvertType.ToLower)
                     .Build();
            // Check if there is a misc record in database, create one if not exist.
            if (!_fsql.Select<Misc>().Any())
                _fsql.Insert(new Misc()).ExecuteAffrows();
        }

        public async Task<Misc> GetMiscAsync()
            => await _fsql.Select<Misc>().FirstAsync();

        public async Task<int> UpdateMiscAsync(Misc misc)
            => await _fsql.Update<Misc>().SetSource(misc).ExecuteAffrowsAsync();

        public async Task<User> GetUserAsync(string name)
            => await _fsql.Select<User>().Where(x => x.Name == name).ToOneAsync();

        public async Task<bool> CheckUserAsync(string name)
            => await _fsql.Select<User>().AnyAsync(x => x.Name == name);

        public async Task<int> CreateUserAsync(User user)
            => (await _fsql.Select<User>().AnyAsync(x => x.Name == user.Name))
            ? 0
            : await _fsql.Insert(user).ExecuteAffrowsAsync();

        public async Task<int> ChangeUserPasswordAsync(string name, string passwordhash)
            => await _fsql.Update<User>().Where(x => x.Name == name).Set(x => x.PasswordHash, passwordhash).ExecuteAffrowsAsync();

        public async Task<Announce> GetAnnounceAsync(int? id = null)
            => id is null
            ? await _fsql.Select<Announce>().OrderByDescending(x => x.Id).FirstAsync()
            : await _fsql.Select<Announce>().Where(x => x.Id == id).ToOneAsync();

        public async Task<Announce> GetActiveAnnounceAsync(int? id = null)
            => id is null
            ? await _fsql.Select<Announce>().OrderByDescending(x => x.Id).Where(x => x.IsActive).FirstAsync()
            : await _fsql.Select<Announce>().Where(x => x.Id == id && x.IsActive).ToOneAsync();
        public async Task<List<Announce>> GetAnnouncesAsync(int page, int pageSize)
            => await _fsql.Select<Announce>().OrderByDescending(x => x.Id).Page(page, pageSize).ToListAsync();

        public async Task<List<Announce>> GetActiveAnnouncesAsync(int page, int pageSize)
            => await _fsql.Select<Announce>().OrderByDescending(x => x.Id).Page(page, pageSize).Where(x => x.IsActive).ToListAsync();

        public async Task<int> CreateAnnounceAsync(Announce a)
            => await _fsql.Insert(a).ExecuteAffrowsAsync();

        public async Task<int> UpdateAnnounceStatusAsync(int id, bool toActive)
            => await _fsql.Update<Announce>().Where(x => x.Id == id).Set(x => x.IsActive, toActive).ExecuteAffrowsAsync();

        public async Task<int> DeleteAnnounceAsync(int id)
            => await _fsql.Delete<Announce>().Where(x => x.Id == id).ExecuteAffrowsAsync();

        public async Task<int> CreateFeedbackAsync(Feedback feedback)
            => await _fsql.Insert(feedback).ExecuteAffrowsAsync();

        public async Task<Feedback> GetFeedbackAsync(int? id = null)
            => id is null
            ? await _fsql.Select<Feedback>().OrderByDescending(x => x.Id).FirstAsync()
            : await _fsql.Select<Feedback>().Where(x => x.Id == id).ToOneAsync();

        public async Task<List<Feedback>> GetFeedbacksAsync(int page, int pageSize)
            => await _fsql.Select<Feedback>().OrderByDescending(x => x.Id).Page(page, pageSize).ToListAsync();

        public async Task<List<Feedback>> GetAllRepliedFeedbacksAsync()
            => await _fsql.Select<Feedback>().Where(x => !string.IsNullOrEmpty(x.Reply)).OrderByDescending(x => x.RepliedTime).ToListAsync();

        public async Task<int> DeleteFeedbackAsync(int id)
            => await _fsql.Delete<Feedback>().Where(x => x.Id == id).ExecuteAffrowsAsync();

        public async Task<int> ReplyFeedbackAsync(int id, string reply, string replyer)
            => await _fsql.Update<Feedback>().Where(x => x.Id == id).Set(x => x.Reply, reply).Set(x => x.ReplierName, replyer).Set(x => x.RepliedTime, DateTime.Now).ExecuteAffrowsAsync();

        public async Task<List<Socket>> GetSocketsAsync(int sectionId)
            => await _fsql.Select<Socket>().Where(x => x.SectionId == sectionId).ToListAsync();

        public async Task<int> UpdateSocketAsync(int seatId, bool isAvailable)
            => await _fsql.Update<Socket>().Where(x => x.SeatId == seatId).Set(x => x.Status, isAvailable ? Socket.SocketStatus.Available : Socket.SocketStatus.Unavailable).ExecuteAffrowsAsync();

        public async Task<int> CreateVersionAsync(Version version)
            => await _fsql.Insert(version).ExecuteAffrowsAsync();

        public async Task<Version> GetVersionAsync(bool isAndroid)
            => await _fsql.Select<Version>().Where(x => x.IsAndroid == isAndroid).OrderByDescending(x => x.CreatedTime).FirstAsync();

        public async Task<int> CreateHttpRequestLogAsync(Request r)
            => await _fsql.Insert(r).ExecuteAffrowsAsync();

        public async Task<int> CreateUsageAsync(Usage u)
            => await _fsql.Insert(u).ExecuteAffrowsAsync();
        public async Task<Dictionary<Usage.FunctionType, int>> GetUsageAsync()
            => await _fsql.Select<Usage>().GroupBy(x => x.Function).ToDictionaryAsync(x => x.Count());
        
    }
}