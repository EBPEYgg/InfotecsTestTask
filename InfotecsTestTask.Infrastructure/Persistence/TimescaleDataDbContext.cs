using InfotecsTestTask.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfotecsTestTask.Infrastructure.Persistence;

public sealed class TimescaleDataDbContext : DbContext
{
    public DbSet<Values> Values => Set<Values>();

    public DbSet<Results> Results => Set<Results>();

    public TimescaleDataDbContext(DbContextOptions<TimescaleDataDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TimescaleDataDbContext).Assembly);
    }
}