using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    public class User
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; } = false;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
}
