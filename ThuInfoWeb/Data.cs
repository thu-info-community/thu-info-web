using ThuInfoWeb.DBModels;

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
            if (!_fsql.Select<Misc>().Any())
                _fsql.Insert(new Misc()).ExecuteAffrows();
        }
        public async Task<Misc> GetMiscAsync()
        {
            return await _fsql.Select<Misc>().FirstAsync();
        }
        public async Task<int> UpdateMiscAsync(Misc misc)
        {
            return await _fsql.Update<Misc>().Set(x => x, misc).ExecuteAffrowsAsync();
        }
        public async Task<User> GetUserAsync(string name)
        {
            return await _fsql.Select<User>().Where(x => x.Name == name).ToOneAsync();
        }
        public async Task<bool> CheckUserAsync(string name)
        {
            return await _fsql.Select<User>().AnyAsync(x => x.Name == name);
        }
        public async Task<int> CreateUserAsync(User user)
        {
            if (await _fsql.Select<User>().AnyAsync(x => x.Name == user.Name)) return 0;
            return await _fsql.Insert(user).ExecuteAffrowsAsync();
        }
        public async Task<int> ChangeUserPasswordAsync(string name, string passwordhash)
        {
            return await _fsql.Update<User>().Where(x => x.Name == name).Set(x => x.PasswordHash, passwordhash).ExecuteAffrowsAsync();
        }
        public async Task<Announce> GetAnnounceAsync(int? id = null)
        {
            return id is null
                ? await _fsql.Select<Announce>().OrderByDescending(x => x.Id).FirstAsync()
                : await _fsql.Select<Announce>().Where(x => x.Id == id).ToOneAsync();
        }
        public async Task<List<Announce>> GetAnnouncesAsync(int page, int pageSize)
        {
            return await _fsql.Select<Announce>().OrderByDescending(x => x.Id).Page(page, pageSize).ToListAsync();
        }
        public async Task<int> CreateAnnounceAsync(Announce a)
        {
            return await _fsql.Insert(a).ExecuteAffrowsAsync();
        }
        public async Task<int> DeleteAnnounceAsync(int id)
        {
            return await _fsql.Delete<Announce>().Where(x => x.Id == id).ExecuteAffrowsAsync();
        }
        public async Task<int> CreateFeedbackAsync(Feedback feedback)
        {
            return await _fsql.Insert(feedback).ExecuteAffrowsAsync();
        }
        public async Task<Feedback> GetFeedbackAsync(int? id = null)
        {
            return id is null
                ? await _fsql.Select<Feedback>().OrderByDescending(x => x.Id).FirstAsync()
                : await _fsql.Select<Feedback>().Where(x => x.Id == id).ToOneAsync();
        }
        public async Task<List<Feedback>> GetFeedbacksAsync(int page, int pageSize)
        {
            return await _fsql.Select<Feedback>().OrderByDescending(x => x.Id).Page(page, pageSize).ToListAsync();
        }
        public async Task<int> DeleteFeedbackAsync(int id)
        {
            return await _fsql.Delete<Feedback>().Where(x => x.Id == id).ExecuteAffrowsAsync();
        }
        public async Task<int> ReplyFeedbackAsync(int id, string reply, string replyer)
        {
            return await _fsql.Update<Feedback>().Where(x => x.Id == id).Set(x => x.Reply, reply).Set(x => x.ReplyerName, replyer).ExecuteAffrowsAsync();
        }
        public async Task<List<Socket>> GetSocketsAsync(int sectionId)
        {
            return await _fsql.Select<Socket>().Where(x => x.SectionId == sectionId).ToListAsync();
        }
    }
}
