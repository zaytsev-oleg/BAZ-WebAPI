using MediatR;

namespace Baz.UseCases.Tasks.Delete;

public sealed record DeleteTaskRequest(int Id) : IRequest<DeleteTaskResponse>;
