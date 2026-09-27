using Baz.Domain;
using Baz.Infrastructure;
using Baz.UseCases.Exceptions;
using Baz.UseCases.Tasks.Delete;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Baz.UseCases.Services;

public class TaskValidationService
{
    private readonly BazDbContext _dbContext;

    public TaskValidationService(BazDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> ValidateOnCreateAsync(TaskItem task, CancellationToken ct, bool throwOnError = false)
    {
        var sb = ValidateCoreFields(task);

        if (task.Id != 0)
        {
            var isAny = await _dbContext.TaskItems.AnyAsync(t => t.Id == task.Id, ct);

            if (isAny)
            {
                sb.AppendLine($"Невозможно создать сущность с Id={task.Id}, т.к. это значение уже есть в БД.");
            }
        }

        var msg = sb.ToString();

        if (throwOnError && msg.Length > 0)
        {
            throw new ValidationException(msg);
        }

        return msg;
    }

    public async Task<string> ValidateOnUpdateAsync(TaskItem task, CancellationToken ct, bool throwOnError = false)
    {
        var sb = ValidateCoreFields(task);

        if (task.Id == 0)
        {
            sb.AppendLine("Поле Id является обязательным для заполнения.");
        }
        else
        {
            var isAny = await _dbContext.TaskItems.AnyAsync(t => t.Id == task.Id, ct);

            if (!isAny)
            {
                sb.AppendLine($"В БД отсутствует сущность с Id={task.Id}.");
            }
        }

        var msg = sb.ToString();

        if (throwOnError && msg.Length > 0)
        {
            throw new ValidationException(msg);
        }

        return msg;
    }

    public async Task<string> ValidateOnDeleteAsync(DeleteTaskRequest request, CancellationToken ct, bool throwOnError = false)
    {
        var isAny = await _dbContext.TaskItems.AnyAsync(t => t.Id == request.Id, ct);

        if (isAny)
        {
            return string.Empty;
        }

        var msg = $"В БД отсутствует сущность с Id={request.Id}.";

        if (throwOnError)
        {
            throw new ValidationException(msg);
        }

        return msg;
    }

    public async Task<string> ValidateOnGetAsync(int id, CancellationToken ct, bool throwOnError = false)
    {
        var isAny = await _dbContext.TaskItems.AnyAsync(t => t.Id == id, ct);

        if (isAny)
        {
            return string.Empty;
        }

        var msg = $"В БД отсутствует сущность с Id={id}.";

        if (throwOnError)
        {
            throw new ValidationException(msg);
        }

        return msg;
    }

    private StringBuilder ValidateCoreFields(TaskItem task)
    {
        var sb = new StringBuilder();

        if (string.IsNullOrEmpty(task.Title?.Trim()))
        {
            sb.AppendLine("Поле Title является обязательным для заполнения.");
        }
        else if (task.Title.Length > 256)
        {
            sb.AppendLine("Длина строки в поле Title не должна превышать 256 символов.");
        }

        if (task.Description != null && task.Description.Length > 512)
        {
            sb.AppendLine("Длина строки в поле Description не должна превышать 512 символов.");
        }

        return sb;
    }
}
