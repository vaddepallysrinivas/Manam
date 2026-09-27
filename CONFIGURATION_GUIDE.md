# Configuration Template for User Management

## appsettings.json Template

Add this to your `appsettings.json` file:

```json
{
  "Jwt": {
    "Secret": "your-very-long-secret-key-minimum-256-bits-or-32-characters-recommended",
    "Issuer": "ManamAPI",
    "Audience": "ManamClient",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "PasswordPolicy": {
    "MinLength": 8,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigits": true,
    "RequireSpecialCharacters": true
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSSL": true,
    "From": "noreply@manam.com",
    "Username": "your-email@gmail.com",
    "Password": "your-app-specific-password"
  },
  "SecurityPolicy": {
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 15,
    "PasswordResetTokenExpiryMinutes": 30,
    "EmailVerificationTokenExpiryHours": 24
  }
}
```

## Program.cs Configuration

Add these configurations to your `Program.cs`:

```csharp
using Manam.API.Middleware;
using Manam.Auth.Extensions;
using Manam.DatabaseClient.Extensions;
using Manam.Services.Extensions;
using Manam.Storage.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

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

// ===== ADD USER MANAGEMENT SERVICES =====
builder.Services.AddUserManagementServices();
// ========================================

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));
});

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

// ===== ADD AUTHENTICATION & AUTHORIZATION =====
app.UseAuthentication();
app.UseAuthorization();
// ==============================================

app.MapControllers();

await app.RunAsync();
```

## appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-O4ATQ75\\MSSQLSERVER2;Database=Manam;Trusted_Connection=true;Encrypt=false;"
  },
  "Jwt": {
    "Secret": "dev-secret-key-minimum-256-bits-or-32-characters-recommended-change-in-production",
    "Issuer": "ManamAPI",
    "Audience": "ManamClient",
    "ExpiryMinutes": 60
  },
  "PasswordPolicy": {
    "MinLength": 8,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigits": true,
    "RequireSpecialCharacters": true
  }
}
```

## appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=PROD-DB-SERVER\\MSSQLSERVER;Database=Manam;User Id=sa;Password=your-password;Encrypt=true;TrustServerCertificate=false;"
  },
  "Jwt": {
    "Secret": "your-production-secret-key-minimum-256-bits-or-32-characters-CHANGE-THIS",
    "Issuer": "ManamAPI",
    "Audience": "ManamClient",
    "ExpiryMinutes": 60
  },
  "PasswordPolicy": {
    "MinLength": 12,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigits": true,
    "RequireSpecialCharacters": true
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSSL": true,
    "From": "noreply@manam.com",
    "Username": "production-email@gmail.com",
    "Password": "production-app-password"
  },
  "SecurityPolicy": {
    "MaxLoginAttempts": 3,
    "LockoutDurationMinutes": 30,
    "PasswordResetTokenExpiryMinutes": 15,
    "EmailVerificationTokenExpiryHours": 24
  }
}
```

## .csproj Dependencies

Make sure your `Manam.Services.csproj` has these references:

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="7.0.0" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Serilog" Version="3.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="4.1.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
</ItemGroup>
```

And `Manam.API.csproj` should have:

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
</ItemGroup>
```

## Environment Variables (Optional)

For production, set these as environment variables instead of in config:

```bash
# JWT Configuration
MANAM_JWT_SECRET=your-production-secret-key
MANAM_JWT_ISSUER=ManamAPI
MANAM_JWT_AUDIENCE=ManamClient
MANAM_JWT_EXPIRY_MINUTES=60

# Database
MANAM_DB_CONNECTION=Server=PROD-DB;Database=Manam;User Id=sa;Password=xyz;

# Email
MANAM_EMAIL_SMTP=smtp.gmail.com
MANAM_EMAIL_PORT=587
MANAM_EMAIL_USERNAME=email@gmail.com
MANAM_EMAIL_PASSWORD=app-password

# Security Policy
MANAM_MAX_LOGIN_ATTEMPTS=3
MANAM_LOCKOUT_MINUTES=30
```

Then update your `Program.cs` to read from environment variables:

```csharp
var jwtSecret = Environment.GetEnvironmentVariable("MANAM_JWT_SECRET") 
    ?? builder.Configuration["Jwt:Secret"];
```

## Security Considerations

1. **JWT Secret**
   - Minimum 256 bits (32 characters)
   - Use a strong random string
   - Store in secure location (not in code)
   - Change in production

2. **Password Policy**
   - Enforce strong passwords (8+ chars)
   - Require uppercase, lowercase, digits, special chars
   - Can be adjusted in config based on security requirements

3. **Email Configuration**
   - Use app-specific passwords for Gmail
   - Enable "Less secure app access" or use App Passwords
   - Store credentials in environment variables, not config files

4. **HTTPS**
   - Always use HTTPS in production
   - Redirect HTTP to HTTPS

5. **Rate Limiting**
   - Implement rate limiting on login endpoint
   - Lock account after failed attempts
   - Configurable in SecurityPolicy section

## Example Environment Setup

### Development
```bash
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=https://localhost:7001
dotnet run
```

### Production
```bash
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=https://0.0.0.0:443
set MANAM_JWT_SECRET=your-secure-key-here
dotnet run
```

---

**Note:** Never commit sensitive configuration to version control. Use environment variables or Azure Key Vault for production secrets.
