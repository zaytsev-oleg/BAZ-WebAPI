using Baz.Domain;
using Baz.UseCases.Dto;

namespace Baz.UseCases.Services;

public static class StaticMappingHelper
{
    public static TaskItemDto MapToDto(this TaskItem task)
    {
        return new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt
        };
    }
}
