using HotelBookingSystem.Application.DTOs.RoomTypes;
using HotelBookingSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _roomTypeService;

    public RoomTypesController(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10)
    {
        var roomTypes =await _roomTypeService.GetAllAsync(pageNumber, pageSize);

        return Ok(roomTypes);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var roomType =await _roomTypeService.GetByIdAsync(id);

        if (roomType is null)
            return NotFound("Room type not found.");

        return Ok(roomType);
    }


    [AllowAnonymous]
    [HttpGet("by-amenity/{amenityId:int}")]
    public async Task<IActionResult> GetByAmenityId(int amenityId)
    {
        var roomTypes =await _roomTypeService.GetByAmenityIdAsync(amenityId);

        return Ok(roomTypes);
    }


    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomTypeDto dto)
    {
        var roomType =await _roomTypeService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = roomType.Id },
            roomType);
    }


    [Authorize(Roles = "Staff,Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,UpdateRoomTypeDto dto)
    {
        var result =await _roomTypeService.UpdateAsync(id, dto);

        if (!result)
            return NotFound("Room type not found.");

        return Ok("Room type updated successfully.");
    }


    [Authorize(Roles = "Staff,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result =await _roomTypeService.DeleteAsync(id);

        if (!result)
            return NotFound("Room type not found.");

        return Ok("Room type deleted successfully.");
    }


    [Authorize(Roles = "Staff,Admin")]
    [HttpPost("{roomTypeId:int}/amenities/{amenityId:int}")]
    public async Task<IActionResult> AddAmenity(int roomTypeId,int amenityId)
    {
        await _roomTypeService.AddAmenityAsync(roomTypeId, amenityId);

        return Ok("Amenity added to room type successfully.");
    }


    [Authorize(Roles = "Staff,Admin")]
    [HttpDelete("{roomTypeId:int}/amenities/{amenityId:int}")]
    public async Task<IActionResult> RemoveAmenity(int roomTypeId,int amenityId)
    {
        var result = await _roomTypeService.RemoveAmenityAsync(roomTypeId, amenityId);

        if (!result)
            return NotFound("This amenity is not assigned to this room type.");

        return Ok("Amenity removed from room type successfully.");
    }
}