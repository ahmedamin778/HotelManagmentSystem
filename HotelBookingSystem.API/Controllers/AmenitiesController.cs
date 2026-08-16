using HotelBookingSystem.Application.DTOs.Amenities;
using HotelBookingSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AmenitiesController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenitiesController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10)
    {
        var amenities =await _amenityService.GetAllAsync(pageNumber, pageSize);

        return Ok(amenities);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var amenity = await _amenityService.GetByIdAsync(id);

        if (amenity is null)
            return NotFound("Amenity not found.");

        return Ok(amenity);
    }


    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAmenityDto dto)
    {
        var amenity = await _amenityService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = amenity.Id },
            amenity);
    }


    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,UpdateAmenityDto dto)
    {
        var result =await _amenityService.UpdateAsync(id, dto);

        return Ok("Amenity updated successfully.");
    }


    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _amenityService.DeleteAsync(id);

        return Ok("Amenity deleted successfully.");
    }
}