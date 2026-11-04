using HotelBookingSystem.Application.DTOs.Rooms;
using HotelBookingSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10,
        [FromQuery] int? roomTypeId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string sortBy = "roomNumber")
    {
        var rooms = await _roomService.GetAllAsync(pageNumber,pageSize,roomTypeId,isActive,sortBy);

        return Ok(rooms);
    }


    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room is null)
            return NotFound("Room not found.");

        return Ok(room);
    }


    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomDto dto)
    {
        var room = await _roomService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = room.Id },
            room);
    }



    [Authorize(Roles = "Staff,Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,UpdateRoomDto dto)
    {
        try
        {
            await _roomService.UpdateAsync(id, dto);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "This room was updated by another user. Refresh the room data and try again."
            });
        }

        return Ok("Room updated successfully.");
    }


    [Authorize(Roles = "Staff,Admin")]
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _roomService.DeactivateAsync(id);

        return Ok("Room deactivated successfully.");
    }


    [Authorize(Roles = "Staff,Admin")]
    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        await _roomService.ActivateAsync(id);

        return Ok("Room activated successfully.");
    }


    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var rooms = await _roomService.GetActiveRoomsAsync();

        return Ok(rooms);
    }
}
