using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Rahal.Api.Extensions;
using Rahal.Api.Middlewares;
using Serilog;
using Shared.Application.Settings;
using Shared.Infrastructure;
using StackExchange.Redis;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//Configure Rate Limit Settings
builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection("RateLimitSettings"));

//Configure Cache Settings
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));

//Inject services
//builder.Services.AddAllModules(builder.Configuration);

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


// Required to access HttpContext in services
builder.Services.AddHttpContextAccessor();


//Register OpenApi Document for internal and public APIs
builder.Services.AddOpenApi("internal", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); //Applying Bearer Security Scheme for the internal API document
});
builder.Services.AddOpenApi("public");

//Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();
    });
});

//////////////////////////////////////////////////////////////////////////////////////////

var app = builder.Build();

// Test Redis connection
try
{
    var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
    var db = redis.GetDatabase();
    await db.PingAsync();
    app.Logger.LogInformation("Redis connection successful");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to connect to Redis");
    throw;
}


app.UseHttpLogging(); //Enable Http Logging

//Run all pending migrations
//await app.ApplyMigrationsAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/openapi/internal.json", "Internal");
        options.SwaggerEndpoint("/openapi/public.json", "Public");
    });

}

app.UseCors();

app.UseRouting(); //Identifying action method based on route
app.UseAuthentication(); //Enable Authentication Middleware
app.UseAuthorization(); //Enable Authorization Middleware
app.MapControllers(); //Execute the filter pipeline (action + filters)

app.UseExceptionHandlingMiddleware();

app.UseHsts(); //Forces the browser to use HTTPS for all requests and responses
app.UseHttpsRedirection();

app.UseAuthorization();


app.Run();
