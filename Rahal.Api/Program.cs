using Microsoft.AspNetCore.HttpLogging;
using Rahal.Api.Extensions;
using Rahal.Api.Middlewares;
using Serilog;
using Shared.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


//Inject services
builder.Services.AddAllModules(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); //Automatically serialize enums as strings in JSON responses
    });


//Register in HTTP Logging
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
});

//Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration)
    =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration) //Assigning the project's logging configs to Serilog configs
    .ReadFrom.Services(services);//Read app services and make them availavle to serilog
});


builder.Services.AddOpenApi();

var app = builder.Build();

app.Logger.LogInformation("Application is starting up...");

//Run all pending migrations
await app.ApplyMigrationsAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandlingMiddleware();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
