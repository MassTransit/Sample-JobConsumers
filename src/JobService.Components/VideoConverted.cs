namespace JobService.Components;

public record VideoConverted
{
    public string? GroupId { get; init; }
    public int Index { get; init; }
    public int Count { get; init; }
}