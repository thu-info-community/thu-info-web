using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    public class Version
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string VersionName { get; set; }
        public string ReleaseNote { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
    }
}
