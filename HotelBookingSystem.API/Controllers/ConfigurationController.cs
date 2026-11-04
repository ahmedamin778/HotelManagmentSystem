using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/configuration")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigurationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet("diagnostic")]
    public IActionResult GetDiagnostic()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        var connectionValues = new DbConnectionStringBuilder { ConnectionString = connectionString };

        return Ok(new
        {
            environmentLabel = _configuration["Diagnostics:EnvironmentLabel"],
            database = new
            {
                server = ReadConnectionValue(connectionValues, "Server", "Data Source"),
                name = ReadConnectionValue(connectionValues, "Database", "Initial Catalog")
            },
            email = new
            {
                host = _configuration["EmailSettings:Host"],
                port = _configuration.GetValue<int>("EmailSettings:Port")
            }
        });
    }

    private static string? ReadConnectionValue(DbConnectionStringBuilder values, params string[] possibleKeys)
    {
        foreach (var key in possibleKeys)
        {
            if (values.TryGetValue(key, out var value))
            {
                return value?.ToString();
            }
        }

        return null;
    }
}
