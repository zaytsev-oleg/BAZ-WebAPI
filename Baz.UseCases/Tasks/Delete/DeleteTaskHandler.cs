using Baz.Infrastructure;
using Baz.UseCases.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baz.UseCases.Tasks.Delete;

public class DeleteTaskHandler : IRequestHandler<DeleteTaskRequest, DeleteTaskResponse>
{
    private readonly BazDbContext _dbContext;
    private readonly TaskValidationService _taskValidationService;

    public DeleteTaskHandler(BazDbContext dbContext, TaskValidationService taskValidationService)
    {
        _dbContext = dbContext;
        _taskValidationService = taskValidationService;
    }

    public async Task<DeleteTaskResponse> Handle(DeleteTaskRequest request, CancellationToken ct)
    {
        await _taskValidationService.ValidateOnDeleteAsync(request, ct, throwOnError: true);

        var task = await _dbContext.TaskItems.FirstAsync(t => t.Id == request.Id, ct);
        _dbContext.Remove(task);

        await _dbContext.SaveChangesAsync(ct);

        return new DeleteTaskResponse();
    }
}
