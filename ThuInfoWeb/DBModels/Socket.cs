using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    public class Socket
    {
        [Column(IsPrimary = true)]
        public int SeatId { get; set; }
        public int SectionId { get; set; }
        public SocketStatus Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public enum SocketStatus
        {
            Unkown,
            Available,
            Unavailable
        }
    }
}
