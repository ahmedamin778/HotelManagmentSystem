using HotelBookingSystem.Api.Middleware;
using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Application.Interfaces.IServices;
using HotelBookingSystem.Application.Mappings;
using HotelBookingSystem.Application.Services;
using HotelBookingSystem.Infrastructure.Data;
using HotelBookingSystem.Infrastructure.Identity;
using HotelBookingSystem.Infrastructure.Repositories;
using HotelBookingSystem.Infrastructure.Services.Email;
using HotelBookingSystem.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotelBookingSystem.Application.Strategies.RoomSorting;
using HotelBookingSystem.Application.Factories.ReportExport;
using HotelBookingSystem.Api.Features.Amenities.CreateAmenity;
using HotelBookingSystem.Api.Features.Amenities.DeleteAmenity;
using HotelBookingSystem.Api.Features.Amenities.GetAmenities;
using HotelBookingSystem.Api.Features.Amenities.GetAmenityById;
using HotelBookingSystem.Api.Features.Amenities.UpdateAmenity;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.OpenApi;
using HotelBookingSystem.Api.Services;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    const string bearerScheme = "Bearer";

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HotelBookingSystem.API",
        Version = "v1"
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "HotelBookingSystem.API",
        Version = "v2"
    });

    options.AddSecurityDefinition(bearerScheme, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter the JWT token returned by POST /api/Auth/login."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(bearerScheme, document, null)] = []
    });
});
builder.Services.AddMemoryCache();



// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("booking-creation", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            GetClientIdentifier(context),
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 3,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 1,
                AutoReplenishment = true
            }));

    options.AddPolicy("dashboard-concurrency", context =>
        RateLimitPartition.GetConcurrencyLimiter(
            GetClientIdentifier(context),
            _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 1,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.OnRejected = async (rejectedContext, cancellationToken) =>
    {
        var httpContext = rejectedContext.HttpContext;
        var clientIdentifier = GetClientIdentifier(httpContext);
        var logger = httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (rejectedContext.Lease.TryGetMetadata(
                MetadataName.RetryAfter,
                out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter = Math.Ceiling(
                retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        logger.LogWarning(
            "Rate limit rejected at {Timestamp}. Client: {ClientIdentifier}, Endpoint: {Endpoint}, Method: {HttpMethod}, StatusCode: {StatusCode}",
            DateTimeOffset.UtcNow,
            clientIdentifier,
            httpContext.Request.Path,
            httpContext.Request.Method,
            httpContext.Response.StatusCode);

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                statusCode = StatusCodes.Status429TooManyRequests,
                message = "Request rejected because the rate limit was exceeded."
            },
            cancellationToken);
    };
});


// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT Key is missing.");

builder.Services.AddAuthentication(options =>
{
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });


// AutoMapper
builder.Services.AddAutoMapper(
    cfg => { },
    typeof(MappingProfile));

// Repositories

builder.Services.AddScoped<IRoomTypeRepository,RoomTypeRepository>();
builder.Services.AddScoped<IRoomRepository,RoomRepository>();
builder.Services.AddScoped<IAmenityRepository,AmenityRepository>();
builder.Services.AddScoped<IBookingRepository,BookingRepository>();
builder.Services.AddScoped<IRoomTypeService,RoomTypeService>();
builder.Services.AddScoped<IRoomService,RoomService>();
builder.Services.AddScoped<GetAmenitiesHandler>();
builder.Services.AddScoped<GetAmenityByIdHandler>();
builder.Services.AddScoped<CreateAmenityHandler>();
builder.Services.AddScoped<UpdateAmenityHandler>();
builder.Services.AddScoped<DeleteAmenityHandler>();
builder.Services.AddScoped<IBookingService,BookingService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IReportExporter, TextReportExporter>();
builder.Services.AddScoped<IReportExporter, JsonReportExporter>();
builder.Services.AddScoped<IReportExporterFactory, ReportExporterFactory>();

builder.Services.AddScoped<IRoomSortingStrategy, RoomNumberSortingStrategy>();
builder.Services.AddScoped<IRoomSortingStrategy, PriceRoomSortingStrategy>();
builder.Services.AddScoped<IRoomSortingStrategy, NameRoomSortingStrategy>();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserEmailService, UserEmailService>();
builder.Services.AddSingleton<DamageReportFileService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
// Apply migrations and seed test data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseSeeder");
    const int maxAttempts = 10;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await DatabaseSeeder.SeedAsync(services);
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                ex,
                "Database is not ready. Attempt {Attempt}/{MaxAttempts}. Retrying in 3 seconds.",
                attempt,
                maxAttempts);

            await Task.Delay(TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration or seed failed after {MaxAttempts} attempts.", maxAttempts);
            throw;
        }
    }
}


// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HotelBookingSystem.API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "HotelBookingSystem.API v2");
    });
}


// HTTP Pipeline

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();

static string GetClientIdentifier(HttpContext context)
{
    return context.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? context.Connection.RemoteIpAddress?.ToString()
        ?? "unknown-client";
}
