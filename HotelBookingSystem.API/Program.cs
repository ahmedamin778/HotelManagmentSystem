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

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
// Seed Identity
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        await SeedIdentityAsync(services);
    }
    catch (Exception)
    {
        Console.Error.WriteLine(
            "Identity seed was skipped because the database is unavailable.");
    }
}


// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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


// Identity Seed
static async Task SeedIdentityAsync(IServiceProvider services)
{
    var roleManager =services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager =services.GetRequiredService<UserManager<ApplicationUser>>();

    // Create Roles
    string[] roles =
    {
        "Admin",
        "Staff",
        "Guest"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result =await roleManager.CreateAsync(new IdentityRole(role));

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }
    }


    // Create Default Admin
    const string adminEmail ="admin@hotel.com";
    const string adminPassword ="Admin@123456";

    var admin =await userManager.FindByEmailAsync(adminEmail);

    if (admin is null)
    {
        admin = new ApplicationUser
        {
            FullName = "Hotel Admin",
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result =
            await userManager.CreateAsync(
                admin,
                adminPassword);

        if (!result.Succeeded)
        {
            throw new Exception(
                string.Join(
                    ", ",
                    result.Errors.Select(
                        x => x.Description)));
        }
    }


    // Add Admin Role
    if (!await userManager.IsInRoleAsync(admin,"Admin"))
    {
        var result =
            await userManager.AddToRoleAsync(
                admin,
                "Admin");

        if (!result.Succeeded)
        {
            throw new Exception(
                string.Join(
                    ", ",
                    result.Errors.Select(
                        x => x.Description)));
        }
    }
}
