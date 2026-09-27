using Baz.Infrastructure;
using Baz.UseCases.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baz.UseCases.Tasks;

public class GetTaskHandler : IRequestHandler<GetTaskRequest, GetTaskResponse>
{
    private readonly BazDbContext _dbContext;
    private readonly TaskValidationService _taskValidationService;

    public GetTaskHandler(BazDbContext dbContext, TaskValidationService taskValidationService)
    {
        _dbContext = dbContext;
        _taskValidationService = taskValidationService;
    }

    public async Task<GetTaskResponse> Handle(GetTaskRequest request, CancellationToken ct)
    {
        if (request.Id.HasValue)
        {
            await _taskValidationService.ValidateOnGetAsync(request.Id.Value, ct, throwOnError: true);
        }

        var tasks = await _dbContext.TaskItems
            .Where(t => request.Id == null || request.Id == t.Id)
            .OrderBy(t => t.Id)
            .Select(t => t.MapToDto())
            .ToArrayAsync(ct);

        return new GetTaskResponse(tasks);
    }
}
