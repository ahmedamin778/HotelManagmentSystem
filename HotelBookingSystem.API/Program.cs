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

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


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
builder.Services.AddScoped<IAmenityService,AmenityService>();
builder.Services.AddScoped<IBookingService,BookingService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReportService, ReportService>();

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

    await SeedIdentityAsync(services);
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

app.UseAuthorization();

app.MapControllers();

app.Run();


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
