using Baz.Infrastructure;
using Baz.UseCases.Exceptions;
using Baz.UseCases.Services;
using Baz.UseCases.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Baz.WebApi.UnitTests.UseCases.Tasks
{
    public class CreateTaskTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<BazDbContext> _dbContextOptions;

        public CreateTaskTests()
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
        public async Task CreateTask_Success()
        {
            using var dbContext = new BazDbContext(_dbContextOptions);
            var taskValidationService = new TaskValidationService(dbContext);

            var handler = new CreateTaskHandler(dbContext, taskValidationService);

            var guid = Guid.NewGuid().ToString();

            var d1 = DateTime.UtcNow;

            await handler.Handle(new CreateTaskRequest(guid, guid, true), default);

            var d2 = DateTime.UtcNow;

            var task = await dbContext.TaskItems.SingleOrDefaultAsync(t => t.Title == guid);

            Assert.NotNull(task);

            Assert.True(task.Id > 0);
            Assert.Equal(guid, task.Title);
            Assert.Equal(guid, task.Description);
            Assert.True(task.IsCompleted);
            Assert.True(task.CreatedAt >= d1 && task.CreatedAt <= d2);
        }

        [Fact]
        public async Task CreateTask_Empty_Title_Error()
        {
            using var dbContext = new BazDbContext(_dbContextOptions);
            var taskValidationService = new TaskValidationService(dbContext);

            var handler = new CreateTaskHandler(dbContext, taskValidationService);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new CreateTaskRequest(string.Empty, "any", true), default));
        }

        [Fact]
        public async Task CreateTask_Title_Max_Length_Exceeded_Error()
        {
            using var dbContext = new BazDbContext(_dbContextOptions);
            var taskValidationService = new TaskValidationService(dbContext);

            var handler = new CreateTaskHandler(dbContext, taskValidationService);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new CreateTaskRequest(new string('a', 256 + 1), "any", true), default));
        }

        [Fact]
        public async Task CreateTask_Description_Max_Length_Exceeded_Error()
        {
            using var dbContext = new BazDbContext(_dbContextOptions);
            var taskValidationService = new TaskValidationService(dbContext);

            var handler = new CreateTaskHandler(dbContext, taskValidationService);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new CreateTaskRequest("any", new string('a', 512 + 1), true), default));
        }

        [Fact]
        public async Task CreateTask_Description_Optional_Success()
        {
            using var dbContext = new BazDbContext(_dbContextOptions);
            var taskValidationService = new TaskValidationService(dbContext);

            var handler = new CreateTaskHandler(dbContext, taskValidationService);

            var guid = Guid.NewGuid().ToString();

            await handler.Handle(new CreateTaskRequest(guid, null, true), default);

            var task = await dbContext.TaskItems.SingleOrDefaultAsync(t => t.Title == guid);

            Assert.NotNull(task);

            Assert.True(task.Id > 0);
            Assert.Null(task.Description);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
