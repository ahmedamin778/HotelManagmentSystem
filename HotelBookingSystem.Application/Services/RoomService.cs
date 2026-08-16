using AutoMapper;
using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.DTOs.Rooms;
using HotelBookingSystem.Application.Exceptions;
using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Application.Interfaces.IServices;
using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;

    public RoomService(IRoomRepository roomRepository, IMapper mapper)
    {
        _roomRepository = roomRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<RoomDto>> GetAllAsync(int pageNumber,int pageSize,int? roomTypeId = null,bool? isActive = null)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var result = await _roomRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            roomTypeId,
            isActive);

        var roomDtos = _mapper.Map<IEnumerable<RoomDto>>(result.Items);

        var totalPages =
            (int)Math.Ceiling(result.TotalCount / (double)pageSize);

        return new PaginatedResultDto<RoomDto>
        {
            Items = roomDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            TotalPages = totalPages
        };
    }

    public async Task<RoomDto?> GetByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdWithRoomTypeAsync(id);

        if (room is null)
            return null;

        return _mapper.Map<RoomDto>(room);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
    {
        var roomNumberExists =await _roomRepository.ExistsByRoomNumberAsync(dto.RoomNumber);

        if (roomNumberExists)
        {
            throw new ConflictException("A room with the same room number already exists.");
        }

        var roomTypeExists =await _roomRepository.RoomTypeExistsAsync(dto.RoomTypeId);

        if (!roomTypeExists)
        {
            throw new NotFoundException("The specified room type does not exist.");
        }

        var room = _mapper.Map<Room>(dto);

        await _roomRepository.AddAsync(room);

        await _roomRepository.SaveChangesAsync();

        room = await _roomRepository.GetByIdWithRoomTypeAsync(room.Id)
            ?? room;

        return _mapper.Map<RoomDto>(room);
    }

    public async Task<bool> UpdateAsync(int id, UpdateRoomDto dto)
    {
        var room = await _roomRepository.GetByIdAsync(id);

        if (room is null)
            throw new NotFoundException("Room not found.");

        var roomNumberExists =await _roomRepository.ExistsByRoomNumberAsync(dto.RoomNumber);

        if (roomNumberExists &&!room.RoomNumber.Equals(dto.RoomNumber,StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("A room with the same room number already exists.");
        }

        var roomTypeExists =await _roomRepository.RoomTypeExistsAsync(dto.RoomTypeId);

        if (!roomTypeExists)
        {
            throw new NotFoundException("The specified room type does not exist.");
        }

        _mapper.Map(dto, room);

        _roomRepository.Update(room);

        await _roomRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);

        if (room is null)
            throw new NotFoundException("Room not found.");

        if (!room.IsActive)
            throw new ConflictException("Room is already inactive.");

        room.IsActive = false;

        _roomRepository.Update(room);

        await _roomRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivateAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);

        if (room is null)
            throw new NotFoundException("Room not found.");

        if (room.IsActive)
            throw new ConflictException("Room is already active.");

        room.IsActive = true;

        _roomRepository.Update(room);

        await _roomRepository.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<RoomDto>> GetActiveRoomsAsync()
    {
        var rooms = await _roomRepository.GetActiveRoomsAsync();

        return _mapper.Map<IEnumerable<RoomDto>>(rooms);
    }
}