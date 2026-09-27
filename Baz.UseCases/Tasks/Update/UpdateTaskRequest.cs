using MediatR;

namespace Baz.UseCases.Tasks;

public sealed record UpdateTaskRequest(int Id, string Title, string? Description, bool IsCompleted) : IRequest<UpdateTaskResponse>;
