using InfotecsTestTask.Infrastructure.Persistence;
using InfotecsTestTask.Web.Extensions;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Initializing application");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder
    .AddSwagger()
    .AddData()
    .AddApplicationServices();

var app = builder.Build();
logger.Info("Starting the app");

if (app.Environment.IsDevelopment())
{
    app.Services.ApplyMigrations();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseErrorHandling();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();