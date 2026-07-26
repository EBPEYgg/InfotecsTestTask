using InfotecsTestTask.Application.Abstractions;
using InfotecsTestTask.Application.Services;
using InfotecsTestTask.Infrastructure.Persistence;
using InfotecsTestTask.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace InfotecsTestTask.Web.Extensions;

public static class ServiceCollectionsExtensions
{
    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Timescale Data API",
                Version = "v1"
            });
        });

        return builder;
    }

    public static WebApplicationBuilder AddData(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<TimescaleDataDbContext>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("TimescaleData")));

        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        //builder.Services.AddScoped<ICsvImportService, CsvImportService>();
        //builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();

        return builder;
    }

    public static WebApplicationBuilder AddRepositories(this WebApplicationBuilder builder)
    {
        //builder.Services.AddScoped<ITimescaleDataRepository, TimescaleDataRepository>();

        return builder;
    }
}
