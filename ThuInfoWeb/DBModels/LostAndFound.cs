using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    public class LostAndFound
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int? TargetId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSolved { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}
