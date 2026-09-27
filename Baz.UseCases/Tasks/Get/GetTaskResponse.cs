using Baz.UseCases.Dto;

namespace Baz.UseCases.Tasks;

public sealed record GetTaskResponse(TaskItemDto[] Tasks);
