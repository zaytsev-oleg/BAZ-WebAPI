using Baz.Domain;
using Baz.Infrastructure;
using Baz.UseCases.Services;
using MediatR;

namespace Baz.UseCases.Tasks;

public class CreateTaskHandler : IRequestHandler<CreateTaskRequest, CreateTaskResponse>
{
    private readonly BazDbContext _dbContext;
    private readonly TaskValidationService _taskValidationService;

    public CreateTaskHandler(BazDbContext dbContext, TaskValidationService taskValidationService)
    {
        _dbContext = dbContext;
        _taskValidationService = taskValidationService;
    }

    public async Task<CreateTaskResponse> Handle(CreateTaskRequest request, CancellationToken ct)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = request.IsCompleted,
            CreatedAt = DateTime.UtcNow
        };

        await _taskValidationService.ValidateOnCreateAsync(task, ct, throwOnError: true);

        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync(ct);

        return new CreateTaskResponse(task.MapToDto());
    }
}
