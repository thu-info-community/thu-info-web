using ThuInfoWeb.Dtos;

namespace ThuInfoWeb.Models;

public class FeedbackViewModel : FeedbackDto
{
    public int Id { get; init; }

    public DateTime CreatedTime { get; init; }

    public string? Reply { get; init; }

    public string? ReplierName { get; init; }

    public DateTime? RepliedTime { get; init; }
}
