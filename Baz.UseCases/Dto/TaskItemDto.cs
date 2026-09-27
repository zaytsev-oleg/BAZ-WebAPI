namespace Baz.UseCases.Dto;

public sealed class TaskItemDto
{
    public int Id { get; set; }
    
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }
}
