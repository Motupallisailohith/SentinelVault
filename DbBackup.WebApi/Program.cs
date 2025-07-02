using DbBackup.WebApi.Data;
using DbBackup.WebApi.Options;
using DbBackup.Core;
using DbBackup.Adapters;
using DbBackup.Storage;
using DbBackup.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Abstractions;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using DbBackup.WebApi.Middleware;
using Microsoft.Extensions.DependencyInjection;
 // Add this line

var builder = WebApplication.CreateBuilder(args);

// 1) Configuration sources
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// 2) EF Core
builder.Services.AddDbContext<BackupConfigContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));

// 3) Profile-store and JsonManifestStore
builder.Services.AddSingleton<DbBackup.Adapters.JsonManifestStore>(sp => 
    new DbBackup.Adapters.JsonManifestStore(builder.Configuration.GetValue<string>("Storage:BasePath") ?? "backups"));

// 4) Core, Adapters, Storage registrars (these bring in BackupOrchestrator, Compressors, etc.)
builder.Services.AddScoped<BackupOrchestrator>();
builder.Services.AddScoped<ICompressor, ZstdStreamCompressor>();
builder.Services.AddScoped<LocalStorageProvider>(sp => 
    new LocalStorageProvider(builder.Configuration.GetValue<string>("Storage:BasePath") ?? "backups"));
builder.Services.AddScoped<IProfileConfigStore, EfProfileConfigStore>();
builder.Services.Configure<BackupOptions>(builder.Configuration.GetSection("Backup"));

// Add logging
builder.Services.AddLogging();

// 5) Register each Db adapter for orchestrator
builder.Services.AddScoped<DbBackup.Core.IDatabaseAdapter>(sp => 
    new MySqlAdapter(
        sp.GetRequiredService<ILogger<MySqlAdapter>>(), 
        sp.GetRequiredService<DbBackup.Adapters.JsonManifestStore>(), 
        sp.GetRequiredService<IStorageProvider>()));

builder.Services.AddScoped<DbBackup.Core.IDatabaseAdapter>(sp => 
    new PostgresAdapter(
        sp.GetRequiredService<ILogger<PostgresAdapter>>(), 
        sp.GetRequiredService<DbBackup.Adapters.JsonManifestStore>()));

builder.Services.AddScoped<DbBackup.Core.IDatabaseAdapter>(sp => 
    new SqliteAdapter(
        sp.GetRequiredService<ILogger<SqliteAdapter>>()));

builder.Services.AddScoped<DbBackup.Core.IDatabaseAdapter>(sp => 
    new MongoDbAdapter(
        sp.GetRequiredService<ILogger<MongoDbAdapter>>()));

// 6) AI config + client + service
builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection("OpenAI"));
builder.Services.AddScoped(sp =>
{
    var opts = sp.GetRequiredService<IOptions<OpenAiOptions>>().Value;
    return new OpenAIClient(opts.ApiKey);
});
builder.Services.AddScoped<IAiExplanationService, AiExplanationService>();

// 7) Storage provider (for logs, manifests, etc)
builder.Services.AddScoped<IStorageProvider, LocalStorageProvider>(sp => 
    new LocalStorageProvider(builder.Configuration.GetValue<string>("Storage:BasePath") ?? "backups"));

// 8) Add health checks
builder.Services.AddHealthChecks();



// 8) Add health checks
builder.Services.AddHealthChecks();

// 9) Configure rate limiting
builder.Services.Configure<DbBackup.WebApi.Options.RateLimitingOptions>(builder.Configuration.GetSection("RateLimiting"));

// 10) Add controllers, Swagger, CORS
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = false;
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return new BadRequestObjectResult(new { errors });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()));

// 11) Configure global exception handling
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(new { errors });
    };
});

var app = builder.Build();

// Initialize database in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BackupConfigContext>();
    
    try
    {
        // Ensure database exists and is migrated
        Console.WriteLine("Initializing database...");
        await db.Database.MigrateAsync();
        Console.WriteLine("Database initialized successfully");
        
        // Create test profile
        Console.WriteLine("Creating test profile...");
        await TestProfile.CreateTestProfileAsync(db);
        Console.WriteLine("Test profile created successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database initialization: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        throw;
    }
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health");
app.UseMiddleware<DbBackup.WebApi.Middleware.RequestResponseLoggingMiddleware>();
app.UseMiddleware<DbBackup.WebApi.Middleware.RateLimitingMiddleware>();
app.UseCors();
app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Configure global exception handling

// Configure global exception handling
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var contextFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (contextFeature != null)
        {
            var error = contextFeature.Error;
            var statusCode = error switch
            {
                ArgumentException => 400,
                UnauthorizedAccessException => 401,
                _ => 500
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { error = error.Message }));
        }
    });
});

app.Run();
