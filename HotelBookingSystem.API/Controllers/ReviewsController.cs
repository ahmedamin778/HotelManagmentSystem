using HotelBookingSystem.Application.DTOs.Reviews;
using HotelBookingSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize(Roles = "Guest")]
    [HttpPost("booking/{bookingId:int}")]
    public async Task<IActionResult> Create(int bookingId,CreateReviewDto dto)
    {
        var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(guestId))
            return Unauthorized();

        var review = await _reviewService.CreateAsync(guestId,bookingId,dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = review.Id },
            review);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var review =await _reviewService.GetByIdAsync(id);

        if (review is null)
            return NotFound("Review not found.");

        return Ok(review);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet("booking/{bookingId:int}")]
    public async Task<IActionResult> GetByBookingId(int bookingId)
    {
        var review = await _reviewService.GetByBookingIdAsync(bookingId);

        if (review is null)
            return NotFound("Review not found for this booking.");

        return Ok(review);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10)
    {
        var reviews = await _reviewService.GetAllAsync(pageNumber,pageSize);

        return Ok(reviews);
    }
}