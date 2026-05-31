using ECommerce.API.Filters;
using Gamification.Application.CQRS.Commands.Achievement;
using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.CQRS.Handlers.Achievements.Commands;
using Gamification.Application.CQRS.Handlers.ExplorerProfiles.Commands;
using Gamification.Application.EventConsumers;
using Gamification.Application.Jobs;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Places.Infrastructure.Search.EventHandlers;
using Rahal.Api.Extensions;
using Rahal.Api.Filters;
using Rahal.Api.Middlewares;
using Serilog;
using Shared.Application.Services;
using Shared.Application.Settings;
using Shared.Infrastructure;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Users.Application.EventHandlers;
using Users.Infrastructure.Search.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

//Register Rate Limiting 
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("per-user", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? httpContext.Connection.RemoteIpAddress?.ToString()
                      ?? "anonymous",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 60
        }));

    options.AddPolicy("otp-resend", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() 
                      ?? "anonymous",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(5),
            PermitLimit = 3
        }));
});

// Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(SendWelcomeEmailHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(UserCreatedEventHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(PlaceCreatedEventHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateAchievementCommandHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviorService<,>));

});

// Register MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DeleteProfileEventConsumer>();
    x.AddConsumer<RestoreProfileEventConsumer>();
    x.AddConsumer<CreateCheckInEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], builder.Configuration["RabbitMQ:VirtualHost"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(30)
        ));

        cfg.ConfigureEndpoints(context);
    });
});


//Configure Cache Settings
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));

//Inject services
builder.Services.AddAllModules(builder.Configuration, builder.Environment);

builder.Services.AddControllers(
    options =>
    {
        options.Filters.Add<ValidationActionFilter>();
        options.Filters.Add<ProfileSetupRequiredFilter>();

    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); //Automatically serialize enums as strings in JSON responses
    });

//Register ValidationActionFilter as a scoped service to enable dependency injection in the filter
builder.Services.AddScoped<ValidationActionFilter>();




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
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
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

    //Refresh leaderboard upon crash
    redis.ConnectionRestored += async (sender, args) =>
    {
        using var scope = app.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new RebuildLeaderboardCommand());
    };
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to connect to Redis");
    throw;
}

//Initializing Meilisearch Indexes at startup
app.InitializeSearchIndexesAsync().GetAwaiter().GetResult();

//Run all pending migrations
await app.ApplyMigrationsAsync();


app.UseHsts();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseHttpLogging(); //Enable Http Logging
app.UseCors();
app.UseRouting(); //Identifying action method based on route
app.UseAuthentication(); //Enable Authentication Middleware
app.UseAuthorization(); //Enable Authorization Middleware
app.UseRateLimiter();


//Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() } //Custom authorization filter to restrict access to the dashboard
}); // Gives you a UI at /hangfire

//RecurringJob.AddOrUpdate<StreakResetBackgroundJob>(
//    "streak-reset",
//    job => job.ExecuteAsync(CancellationToken.None),
//    Cron.Daily(0));

//// Initial leaderboard build
//using var scope = app.Services.CreateScope();
//var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
//await mediator.Send(new RebuildLeaderboardCommand());

app.UseExceptionHandlingMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/openapi/internal.json", "Internal");
        options.SwaggerEndpoint("/openapi/public.json", "Public");
    });

}


app.MapControllers(); //Execute the filter pipeline (action + filters)

app.Run();
