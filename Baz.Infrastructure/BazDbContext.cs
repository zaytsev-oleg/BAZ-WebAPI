using Baz.Domain;
using Microsoft.EntityFrameworkCore;

namespace Baz.Infrastructure;

public class BazDbContext : DbContext
{
    public BazDbContext(DbContextOptions<BazDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BazDbContext).Assembly);
    }

    public DbSet<TaskItem> TaskItems { get; set; }
}
