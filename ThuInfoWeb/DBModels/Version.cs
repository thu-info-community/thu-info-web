using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    public class Version
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string VersionName { get; set; }
        [Column(StringLength = -1)]
        public string ReleaseNote { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool IsAndroid { get; set; }
    }
}
