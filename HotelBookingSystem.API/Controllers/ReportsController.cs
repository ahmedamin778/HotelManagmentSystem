using HotelBookingSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var report = await _reportService.GetDashboardAsync();

        return Ok(report);
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