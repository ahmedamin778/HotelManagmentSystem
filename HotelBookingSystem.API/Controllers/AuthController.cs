using HotelBookingSystem.Application.DTOs.Auth;
using HotelBookingSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }


    
    //To Guest 
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser is not null)
        {
            return Conflict("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description);
            return BadRequest(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Guest");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = roleResult.Errors.Select(x => x.Description);

            return StatusCode(500,errors);
        }

        return Ok(new
        {
            message = "Guest registered successfully.",
            userId = user.Id
        });
    }


    // Login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user =await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var result =await _signInManager.CheckPasswordSignInAsync(user,dto.Password,false);

        if (!result.Succeeded)
        {
            return Unauthorized("Invalid email or password.");
        }

        var roles =await _userManager.GetRolesAsync(user);

        // Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier,user.Id),
            new Claim(ClaimTypes.Name,user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email,user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role,role));
        }
        // JWT Configuration
        var jwtKey =_configuration["Jwt:Key"];

        var jwtIssuer =_configuration["Jwt:Issuer"];
        var jwtAudience =_configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            return StatusCode(500,"JWT Key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            return StatusCode(500,"JWT Issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtAudience))
        {
            return StatusCode(500,"JWT Audience is not configured.");
        }

        // Create JWT
        var key =new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var credentials =new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

        var jwt =new JwtSecurityTokenHandler()
            .WriteToken(token);

        // Response
        return Ok(new
        {
            message = "Login successful.",

            token = jwt,

            userId = user.Id,

            fullName = user.FullName,

            email = user.Email,

            roles = roles
        });
    }


    // Staff Register 
    [Authorize(Roles = "Admin")]
    [HttpPost("register-staff")]
    public async Task<IActionResult> RegisterStaff(RegisterStaffDto dto)
    {
        var existingUser =await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser is not null)
        {
            return Conflict("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(x => x.Description);

            return BadRequest(errors);
        }

        var roleResult =await _userManager.AddToRoleAsync(user,"Staff");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = roleResult.Errors.Select(x => x.Description);

            return StatusCode(500, errors);
        }

        return Ok(new
        {
            message = "Staff registered successfully.",
            userId = user.Id
        });
    }
}