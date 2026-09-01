using HotelBookingSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using HotelBookingSystem.Application.Factories.ReportExport;
using System.Text;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IReportExporterFactory _reportExporterFactory;

    public ReportsController(IReportService reportService,IReportExporterFactory reportExporterFactory)
    {
        _reportService = reportService;
        _reportExporterFactory = reportExporterFactory;
    }

    
    [HttpGet("dashboard")]
    [EnableRateLimiting("dashboard-concurrency")]
    public async Task<IActionResult> GetDashboard()
    {
        var report = await _reportService.GetDashboardAsync();

        return Ok(report);
    }

    [HttpGet("dashboard/export")]
    public async Task<IActionResult> ExportDashboard([FromQuery] string format = "text")
    {
        var report = await _reportService.GetDashboardAsync();
        var exporter = _reportExporterFactory.Create(format);
        var content = exporter.Export(report);

        return File(
            Encoding.UTF8.GetBytes(content),
            exporter.ContentType,
            $"dashboard-report.{exporter.FileExtension}");
    }

    
    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancy()
    {
        var report = await _reportService.GetOccupancyAsync();

        return Ok(report);
    }

 


    [HttpGet("best-reviewed-room-types")]
    public async Task<IActionResult> GetBestReviewedRoomTypes()
    {
        var report =
            await _reportService.GetBestReviewedRoomTypesAsync();

        return Ok(report);
    }

   

    [HttpGet("guests/{guestId}/booking-history")]
    public async Task<IActionResult> GetGuestBookingHistory(string guestId)
    {
        var bookings =await _reportService.GetGuestBookingHistoryAsync(guestId);

        return Ok(bookings);
    }

    

}
