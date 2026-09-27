using MediatR;

namespace Baz.UseCases.Tasks;

public sealed record GetTaskRequest(int? Id = null) : IRequest<GetTaskResponse>;
