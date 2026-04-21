using Microsoft.EntityFrameworkCore;
using Smart_Farm.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Smart_Farm.Infrastructure.DependencyInjection;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Optional local override (never commit secrets)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// ===================== CONFIG VALIDATION =====================

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must be configured and at least 32 characters. Set it in appsettings.json or User Secrets.");
}

var connectionString = builder.Configuration.GetConnectionString("farContext");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:farContext is not configured.");
}

// ===================== SERVICES =====================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT من استجابة POST api/Authentication/login — الصق الرمز في حقل Authorize."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<farContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<farContext>()
    .AddSignInManager();

builder.Services.AddHttpClient("PlantNet", client =>
{
    client.BaseAddress = new Uri("https://my-api.plantnet.org/");
    client.Timeout = TimeSpan.FromMinutes(2);
});

builder.Services
    .AddApplicationLayer()
    .AddInfrastructureLayer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

var debugLogPath = Path.Combine(Path.GetTempPath(), "smart-farm-debug-4256b6.log");
var debugRunId = $"run-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

void WriteDebugLog(string hypothesisId, string location, string message, object data)
{
    var payload = new
    {
        sessionId = "4256b6",
        runId = debugRunId,
        hypothesisId,
        location,
        message,
        data,
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    };
    try
    {
        var dir = Path.GetDirectoryName(debugLogPath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        File.AppendAllText(debugLogPath, JsonSerializer.Serialize(payload) + Environment.NewLine);
    }
    catch
    {
        // Best-effort debug logging; never crash the app for this.
    }
}

bool CanBindToPort(int port)
{
    try
    {
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        return true;
    }
    catch
    {
        return false;
    }
}

int? FindNextAvailablePort(int startPort, int attempts = 30)
{
    for (var p = startPort; p < startPort + attempts; p++)
    {
        if (CanBindToPort(p))
            return p;
    }

    return null;
}

// ===================== PIPELINE =====================

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (UnauthorizedAccessException)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
});

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var existing)
        && !string.IsNullOrWhiteSpace(existing)
        ? existing.ToString()
        : Guid.NewGuid().ToString("N");

    context.Response.Headers["X-Correlation-Id"] = correlationId;

    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestTrace");
    using (logger.BeginScope(new Dictionary<string, object?>
           {
               ["correlationId"] = correlationId,
               ["method"] = context.Request.Method,
               ["path"] = context.Request.Path.Value
           }))
    {
        try
        {
            await next();
        }
        finally
        {
            logger.LogInformation("HTTP {Method} {Path} -> {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
        }
    }
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<farContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("MigrationBootstrap");

    // Baseline existing databases scaffolded manually so EF can apply newer migrations (Identity).
    var bootstrapSql = """
        IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
        BEGIN
            CREATE TABLE [__EFMigrationsHistory] (
                [MigrationId] nvarchar(150) NOT NULL,
                [ProductVersion] nvarchar(32) NOT NULL,
                CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
            );
        END;
        
        IF OBJECT_ID(N'[Disease]') IS NOT NULL
           AND NOT EXISTS (
               SELECT 1
               FROM [__EFMigrationsHistory]
               WHERE [MigrationId] = N'20260414025037_InitialCreate'
           )
        BEGIN
            INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
            VALUES (N'20260414025037_InitialCreate', N'9.0.0');
        END;

        IF OBJECT_ID(N'[AspNetRoles]') IS NOT NULL
           AND NOT EXISTS (
               SELECT 1
               FROM [__EFMigrationsHistory]
               WHERE [MigrationId] = N'20260417022034_AddIdentityAuthSchema'
           )
        BEGIN
            INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
            VALUES (N'20260417022034_AddIdentityAuthSchema', N'9.0.0');
        END;
        """;

    try
    {
        db.Database.ExecuteSqlRaw(bootstrapSql);
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration bootstrap failed.");
        throw;
    }
}


    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

// IMPORTANT ORDER
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrWhiteSpace(aspnetcoreUrls))
{
    var requestedUrls = aspnetcoreUrls
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();

    var adjustedUrls = new List<string>();
    var changed = false;

    foreach (var rawUrl in requestedUrls)
    {
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
        {
            adjustedUrls.Add(rawUrl);
            continue;
        }

        var isLocalhost = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                          || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                          || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);

        if (isLocalhost && !CanBindToPort(uri.Port))
        {
            var fallbackPort = FindNextAvailablePort(uri.Port + 1);
            if (fallbackPort.HasValue)
            {
                var uriBuilder = new UriBuilder(uri) { Port = fallbackPort.Value };
                var newUrl = uriBuilder.Uri.ToString().TrimEnd('/');
                adjustedUrls.Add(newUrl);
                changed = true;

                // #region agent log
                WriteDebugLog(
                    "H5",
                    "Program.cs:port-fallback",
                    "Detected occupied port and selected fallback",
                    new { requestedUrl = rawUrl, fallbackUrl = newUrl });
                // #endregion
            }
            else
            {
                adjustedUrls.Add(rawUrl);
            }
        }
        else
        {
            adjustedUrls.Add(rawUrl);
        }
    }

    if (changed)
    {
        app.Urls.Clear();
        foreach (var url in adjustedUrls.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            app.Urls.Add(url);
        }
    }
}

// #region agent log
WriteDebugLog(
    "H1-H2",
    "Program.cs:startup-before-run",
    "App startup context before app.Run",
    new
    {
        processId = Environment.ProcessId,
        environment = app.Environment.EnvironmentName,
        aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS"),
        launchProfile = Environment.GetEnvironmentVariable("DOTNET_LAUNCH_PROFILE")
    });
// #endregion

try
{
    app.Run();
}
catch (Exception ex)
{
    // #region agent log
    WriteDebugLog(
        "H1",
        "Program.cs:app-run-exception",
        "app.Run failed",
        new
        {
            exceptionType = ex.GetType().FullName,
            ex.Message,
            innerMessage = ex.InnerException?.Message
        });
    // #endregion
    throw;
}