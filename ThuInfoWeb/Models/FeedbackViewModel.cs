using ThuInfoWeb.Dtos;

namespace ThuInfoWeb.Models
{
    public class FeedbackViewModel : FeedbackDto
    {
        public int Id { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Reply { get; set; } = string.Empty;
        public string ReplyerName { get; set; } = string.Empty;
        public DateTime? RepliedTime { get; set; }
    }
}
