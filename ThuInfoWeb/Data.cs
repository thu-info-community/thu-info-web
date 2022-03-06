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
                 .UseMonitorCommand(cmd => Console.WriteLine(cmd.CommandText))
                 .Build();
        }
        public async Task<User> GetUserAsync(string name)
        {
            return await _fsql.Select<User>().Where(x => x.Name == name).ToOneAsync();
        }
        public async Task<int> CreateUserAsync(User user)
        {
            if (await _fsql.Select<User>().AnyAsync(x => x.Name == user.Name)) return 0;
            return await _fsql.Insert(user).ExecuteAffrowsAsync();
        }
    }
}
