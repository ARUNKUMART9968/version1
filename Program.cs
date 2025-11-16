using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BoticAPI.Data;
using BoticAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var configuration = builder.Configuration;

// Add DbContext - POSTGRESQL
builder.Services.AddDbContext<BoticDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
);

// Add application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBotService, BotService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Configure JWT Authentication
var jwtKey = configuration["Jwt:Key"];
var jwtIssuer = configuration["Jwt:Issuer"];
var jwtAudience = configuration["Jwt:Audience"];
var key = Encoding.ASCII.GetBytes(jwtKey ?? string.Empty);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Controllers
builder.Services.AddControllers();

// CORS - for debug AllowAnyOrigin; in production limit to your frontend origin(s)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Botic Application Tracking System API",
        Version = "v1",
        Description = "Hybrid ATS with automated bot updates and manual admin management"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Handle PORT for production (Render, Railway, and other platforms)
if (!builder.Environment.IsDevelopment())
{
    var portVar = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(portVar) && int.TryParse(portVar, out var port))
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(port);
        });
    }
}

var app = builder.Build();

// If behind Render or other proxy:
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Optional: normalize multiple slashes in the incoming path (fixes //auth/login)
app.Use(async (context, next) =>
{
    var p = context.Request.Path.Value;
    if (!string.IsNullOrEmpty(p) && p.Contains("//"))
    {
        var newPath = System.Text.RegularExpressions.Regex.Replace(p, "/{2,}", "/");
        context.Request.Path = new Microsoft.AspNetCore.Http.PathString(newPath);
    }
    await next();
});

// HTTPS redirect early (optional but recommended for production)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// MUST call UseRouting() before UseCors and before authentication/authorization
app.UseRouting();

// CORS - must come after UseRouting and before UseAuthentication/UseAuthorization
app.UseCors("AllowAll");

// Short-circuit OPTIONS preflight (optional safety - returns 204 quickly)
app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Swagger (visible in dev and prod)
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Botic ATS API v1");
        c.RoutePrefix = "swagger";
    });
}

// Map controllers / endpoints
app.MapControllers();

// Health check endpoint for Render
app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
})
.WithName("Health")
.WithOpenApi()
.AllowAnonymous();

// Database initialization
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<BoticDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            logger.LogError("❌ CONNECTION STRING IS NULL!");
            logger.LogError("Make sure 'ConnectionStrings__DefaultConnection' is set in environment variables");
            logger.LogError("Current environment: {environment}", app.Environment.EnvironmentName);

            if (!app.Environment.IsProduction())
            {
                throw new InvalidOperationException("Database connection string is required");
            }
        }
        else
        {
            logger.LogInformation("✅ Connection string found");
            logger.LogInformation("Applying database migrations...");

            try
            {
                dbContext.Database.Migrate();
                logger.LogInformation("✅ Database migrations applied successfully");
            }
            catch (Exception migrationEx)
            {
                logger.LogError(migrationEx, "❌ Database migration failed");
                throw;
            }

            logger.LogInformation("Seeding database...");
            try
            {
                SeedData.Initialize(dbContext);
                logger.LogInformation("✅ Database seeding completed successfully");
            }
            catch (Exception seedEx)
            {
                logger.LogError(seedEx, "❌ Database seeding failed");
                throw;
            }

            logger.LogInformation("✅ Database initialization completed successfully");
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "❌ An error occurred during database initialization");

    if (!app.Environment.IsProduction())
    {
        throw;
    }
    else
    {
        logger.LogError("Application will continue but database may not be properly initialized");
    }
}

app.Run();