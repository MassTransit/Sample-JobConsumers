namespace JobService.Components;

public record ConvertVideo
{
    public string? GroupId { get; init; }
    public int Index { get; init; }
    public int Count { get; init; }
    public string? Path { get; init; }

    public ICollection<VideoDetail>? Details { get; init; } = new List<VideoDetail>();
}