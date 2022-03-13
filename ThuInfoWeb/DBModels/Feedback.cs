using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    public class Feedback
    {
        [Column(IsPrimary = true,IsIdentity = true)]
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedTime { get; set; }
        public string AppVersion { get; set; }
        public string OS { get; set; }
        public string? Reply { get; set; } = string.Empty;
    }
}
