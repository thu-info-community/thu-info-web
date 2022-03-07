using ThuInfoWeb.DBModels;

namespace ThuInfoWeb
{
    public class Data
    {
        private readonly IFreeSql _fsql;
        public Data(string connectionString)
        {
            _fsql = new FreeSql.FreeSqlBuilder()
                 .UseConnectionString(FreeSql.DataType.PostgreSQL, connectionString)
                 .UseAutoSyncStructure(true)
                 .Build();
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
        public async Task<Announce> GetAnnounceAsync(int id = 0)
        {
            return id == 0
                ? await _fsql.Select<Announce>().OrderByDescending(x => x.Id).FirstAsync()
                : await _fsql.Select<Announce>().Where(x => x.Id == id).ToOneAsync();
        }
        public async Task<List<Announce>> GetAnnouncesAsync(int page,int pageSize)
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
    }
}
