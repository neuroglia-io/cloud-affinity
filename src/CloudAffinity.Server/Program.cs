using CloudAffinity.Application;
using CloudAffinity.Application.Configuration;
using CloudAffinity.Application.Services;
using Hylo.Infrastructure;
using Hylo.Providers.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(CorrelatorOptions.EnvironmentVariablePrefix);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHylo(builder.Configuration, builder =>
{
    builder
        .UseDatabaseInitializer<DatabaseInitializer>()
        .UseRedis("localhost");
});
builder.Services.AddCloudAffinity(builder.Configuration);
var app = builder.Build();

app.MapControllers();

app.Run();
