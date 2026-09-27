using Baz.Domain;
using Baz.Infrastructure;
using Baz.UseCases.Services;
using MediatR;

namespace Baz.UseCases.Tasks;

public class UpdateTaskHandler : IRequestHandler<UpdateTaskRequest, UpdateTaskResponse>
{
    private readonly BazDbContext _dbContext;
    private readonly TaskValidationService _taskValidationService;

    public UpdateTaskHandler(BazDbContext dbContext, TaskValidationService taskValidationService)
    {
        _dbContext = dbContext;
        _taskValidationService = taskValidationService;
    }

    public async Task<UpdateTaskResponse> Handle(UpdateTaskRequest request, CancellationToken ct)
    {
        var task = new TaskItem
        {
            Id = request.Id,
            Title = request.Title,
            Description = request.Description,
            IsCompleted = request.IsCompleted,
            CreatedAt = DateTime.UtcNow
        };

        await _taskValidationService.ValidateOnUpdateAsync(task, ct, throwOnError: true);

        _dbContext.Update(task);
        await _dbContext.SaveChangesAsync(ct);

        return new UpdateTaskResponse(task.MapToDto());
    }
}
