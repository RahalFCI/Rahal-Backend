using Rahal.Api.Extensions;
using Rahal.Api.Middlewares;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


//Inject services
builder.Services.AddAllModules(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

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
