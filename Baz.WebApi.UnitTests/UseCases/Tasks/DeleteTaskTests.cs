using Baz.Infrastructure;
using Baz.UseCases.Exceptions;
using Baz.UseCases.Services;
using Baz.UseCases.Tasks;
using Baz.UseCases.Tasks.Delete;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Baz.WebApi.UnitTests.UseCases.Tasks;

public class DeleteTaskTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<BazDbContext> _dbContextOptions;

    public DeleteTaskTests()
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
    public async Task DeleteTask_Success()
    {
        var guids = await SeedDataAsync();

        using var dbContext = new BazDbContext(_dbContextOptions);
        var taskValidationService = new TaskValidationService(dbContext);

        var seedTotalCount = await dbContext.TaskItems.CountAsync();

        Assert.True(seedTotalCount > 0);
        Assert.Equal(guids.Length, seedTotalCount);

        var guid = guids[0];
        var task = await dbContext.TaskItems.SingleAsync(t => t.Title == guid);

        var deleteHandler = new DeleteTaskHandler(dbContext, taskValidationService);

        await deleteHandler.Handle(new DeleteTaskRequest(task.Id), default);

        var newTotalCount = await dbContext.TaskItems.CountAsync();

        Assert.Equal(seedTotalCount - 1, newTotalCount);

        var deletedTask = await dbContext.TaskItems.SingleOrDefaultAsync(t => t.Id == task.Id);

        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task DeleteTask_Error()
    {
        var guids = await SeedDataAsync();

        using var dbContext = new BazDbContext(_dbContextOptions);
        var taskValidationService = new TaskValidationService(dbContext);

        var seedTotalCount = await dbContext.TaskItems.CountAsync();

        Assert.True(seedTotalCount > 0);
        Assert.Equal(guids.Length, seedTotalCount);

        var maxId = await dbContext.TaskItems.Select(t => t.Id).MaxAsync();

        var deleteHandler = new DeleteTaskHandler(dbContext, taskValidationService);

        await Assert.ThrowsAsync<ValidationException>(() => deleteHandler.Handle(new DeleteTaskRequest(maxId + 1), default));

        var newTotalCount = await dbContext.TaskItems.CountAsync();

        Assert.Equal(seedTotalCount, newTotalCount);
    }

    private async Task<string[]> SeedDataAsync(int n = 3)
    {
        using var dbContext = new BazDbContext(_dbContextOptions);
        var taskValidationService = new TaskValidationService(dbContext);

        var createHandler = new CreateTaskHandler(dbContext, taskValidationService);

        var guids = new string[n];

        for (var i = 0; i < n; i++)
        {
            var guid = Guid.NewGuid().ToString();
            await createHandler.Handle(new CreateTaskRequest(guid, guid, i % 2 == 1), default);

            guids[i] = guid;
        }

        return guids;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
