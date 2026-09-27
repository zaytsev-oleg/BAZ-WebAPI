using Baz.Infrastructure;
using Baz.UseCases.Exceptions;
using Baz.UseCases.Services;
using Baz.UseCases.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Baz.WebApi.UnitTests.UseCases.Tasks;

public class GetTaskTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<BazDbContext> _dbContextOptions;

    public GetTaskTests()
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
    public async Task GetTask_Single_Success()
    {
        var guids = await SeedDataAsync();

        using var dbContext = new BazDbContext(_dbContextOptions);
        var taskValidationService = new TaskValidationService(dbContext);

        var getHandler = new GetTaskHandler(dbContext, taskValidationService);

        var guid = guids[0];
        var task = await dbContext.TaskItems.SingleOrDefaultAsync(t => t.Title == guid);

        Assert.NotNull(task);

        var response = await getHandler.Handle(new GetTaskRequest(task.Id), default);

        Assert.NotNull(response.Tasks);
        Assert.True(response.Tasks.Length == 1);
        Assert.Equal(task.Id, response.Tasks[0].Id);
        Assert.Equal(task.Title, response.Tasks[0].Title);
    }

    [Fact]
    public async Task GetTask_All_Success()
    {
        var guids = await SeedDataAsync();

        using var dbContext = new BazDbContext(_dbContextOptions);
        var taskValidationService = new TaskValidationService(dbContext);

        var getHandler = new GetTaskHandler(dbContext, taskValidationService);
        var response = await getHandler.Handle(new GetTaskRequest(), default);

        Assert.NotNull(response.Tasks);
        Assert.Equal(response.Tasks.Length, guids.Length);
        Assert.Equal(guids, response.Tasks.Select(t => t.Title));
    }


    [Fact]
    public async Task GetTask_Single_Error()
    {
        var guids = await SeedDataAsync();

        using var dbContext = new BazDbContext(_dbContextOptions);
        var taskValidationService = new TaskValidationService(dbContext);

        var getHandler = new GetTaskHandler(dbContext, taskValidationService);

        var maxId = await dbContext.TaskItems.Select(t => t.Id).MaxAsync();

        Assert.Equal(guids.Length, maxId);

        await Assert.ThrowsAsync<ValidationException>(() => getHandler.Handle(new GetTaskRequest(maxId + 1), default));
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
