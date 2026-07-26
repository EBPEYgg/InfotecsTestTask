using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfotecsTestTask.Infrastructure.Persistence;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        using TimescaleDataDbContext context = scope.ServiceProvider.GetRequiredService<TimescaleDataDbContext>();
        context.Database.Migrate();
    }
}