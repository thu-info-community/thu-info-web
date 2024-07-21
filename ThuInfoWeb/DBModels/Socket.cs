using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

public class Socket
{
    public enum SocketStatus
    {
        Unknown,
        Available,
        Unavailable
    }

    [Column(IsPrimary = true)]
    public int SeatId { get; set; }

    [Column(IsNullable = false)]
    public int SectionId { get; set; }

    [Column(IsNullable = false)]
    public SocketStatus Status { get; set; }

    [Column(IsNullable = false)]
    public DateTime CreatedTime { get; set; }

    [Column(IsNullable = false)]
    public DateTime UpdatedTime { get; set; }
}
