using HotelBookingSystem.Application.DTOs.Bookings;
using HotelBookingSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> Create(CreateBookingDto dto)
    {
        var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(guestId))
            return Unauthorized("User identity could not be determined.");

        var booking = await _bookingService.CreateAsync(guestId, dto);

        return Ok(booking);
    }


    [HttpGet("my")]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> GetMyBookings()
    {
        var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(guestId))
            return Unauthorized();

        var bookings = await _bookingService.GetMyBookingsAsync(guestId);

        return Ok(bookings);
    }


    [HttpGet("my/{bookingId:int}")]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> GetMyBookingById(int bookingId)
    {
        var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(guestId))
            return Unauthorized();

        var booking = await _bookingService.GetMyBookingByIdAsync(guestId, bookingId);

        if (booking is null)
            return NotFound("Booking not found.");

        return Ok(booking);
    }



    [HttpPatch("my/{bookingId:int}/cancel")]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> CancelMyBooking(int bookingId)
    {
        var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(guestId))
            return Unauthorized();

        var result = await _bookingService.CancelMyBookingAsync(guestId,bookingId);

        if (!result)
            return NotFound("Booking not found.");

        return Ok("Booking cancelled successfully.");
    }


    [HttpGet]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10)
    {
        var bookings = await _bookingService.GetAllAsync(pageNumber, pageSize);

        return Ok(bookings);
    }


    [HttpGet("{id:int}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);

        if (booking is null)
            return NotFound("Booking not found.");

        return Ok(booking);
    }


    [HttpPatch("{id:int}/confirm")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> Confirm(int id)
    {
        var result = await _bookingService.ConfirmAsync(id);

        if (!result)
            return NotFound("Booking not found.");

        return Ok("Booking confirmed successfully.");
    }


    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _bookingService.CancelAsync(id);

        if (!result)
            return NotFound("Booking not found.");

        return Ok("Booking cancelled successfully.");
    }


    [HttpPatch("{id:int}/check-in")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> CheckIn(int id)
    {
        var result = await _bookingService.CheckInAsync(id);

        if (!result)
            return NotFound("Booking not found.");

        return Ok("Guest checked in successfully.");
    }


    [HttpPatch("{id:int}/check-out")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> CheckOut(int id)
    {
        var result = await _bookingService.CheckOutAsync(id);

        if (!result)
            return NotFound("Booking not found.");

        return Ok("Guest checked out successfully.");
    }


    [HttpGet("available")]
    [Authorize(Roles = "Guest,Staff,Admin")]
    public async Task<IActionResult> GetAvailableRooms([FromQuery] DateTime checkInDate,[FromQuery] DateTime checkOutDate)
    {
        var rooms = await _bookingService.GetAvailableRoomsAsync(checkInDate, checkOutDate);

        return Ok(rooms);
    }
}