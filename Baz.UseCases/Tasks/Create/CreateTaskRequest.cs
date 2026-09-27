using MediatR;

namespace Baz.UseCases.Tasks;

public sealed record CreateTaskRequest(string Title, string? Description, bool IsCompleted) : IRequest<CreateTaskResponse>;
