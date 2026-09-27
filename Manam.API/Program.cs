using Manam.API.Middleware;
using Manam.Auth.Extensions;
using Manam.DatabaseClient.Extensions;
using Manam.Services.Extensions;
using Manam.Storage.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/manam-api-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Manam.API")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services from all projects using extension methods
builder.Services.AddDatabaseClientServices(connectionString);
builder.Services.AddStorageServices();
builder.Services.AddBusinessServices();
builder.Services.AddAuthenticationServices();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add custom middleware
app.UseExceptionHandling();
app.UseRequestCorrelation();
app.UseRequestLogging();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();

await app.RunAsync();
