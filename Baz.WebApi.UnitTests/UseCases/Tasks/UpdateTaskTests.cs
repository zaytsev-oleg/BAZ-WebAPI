using Baz.Infrastructure;
using Baz.UseCases.Services;
using Baz.UseCases.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Baz.WebApi.UnitTests.UseCases.Tasks;

public class UpdateTaskTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<BazDbContext> _dbContextOptions;

    public UpdateTaskTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _dbContextOptions = new DbContextOptionsBuilder<BazDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var dbContext = new BazDbContext(_dbContextOptions);
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task UpdateTask_Success()
    {
        using var dbContext = new BazDbContext(_dbContextOptions);
        var taskValidationService = new TaskValidationService(dbContext);

        var createHandler = new CreateTaskHandler(dbContext, taskValidationService);

        var guid = Guid.NewGuid().ToString();

        await createHandler.Handle(new CreateTaskRequest(guid, guid, true), default);

        var savedTask = await dbContext.TaskItems.FirstAsync(t => t.Title == guid);
        dbContext.Entry(savedTask).State = EntityState.Detached;

        var updateHandler = new UpdateTaskHandler(dbContext, taskValidationService);
        await updateHandler.Handle(new UpdateTaskRequest(savedTask.Id, savedTask.Title, savedTask.Description, !savedTask.IsCompleted), default);

        var updatedTask = await dbContext.TaskItems.FirstAsync(t => t.Title == guid);

        Assert.Equal(savedTask.Id, updatedTask.Id);
        Assert.Equal(savedTask.Title, updatedTask.Title);
        Assert.Equal(savedTask.Description, updatedTask.Description);
        Assert.Equal(!savedTask.IsCompleted, updatedTask.IsCompleted);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
