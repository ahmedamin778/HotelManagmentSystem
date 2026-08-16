using AutoMapper;
using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.DTOs.RoomTypes;
using HotelBookingSystem.Application.Exceptions;
using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Application.Interfaces.IServices;
using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Services;

public class RoomTypeService : IRoomTypeService
{
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IMapper _mapper;

    public RoomTypeService(IRoomTypeRepository roomTypeRepository,IMapper mapper)
    {
        _roomTypeRepository = roomTypeRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<RoomTypeDto>> GetAllAsync(int pageNumber,int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var result =await _roomTypeRepository.GetPagedAsync(pageNumber,pageSize);

        var items = result.Items;

        var totalCount = result.TotalCount;

        var roomTypeDtos =_mapper.Map<IEnumerable<RoomTypeDto>>(items);

        var totalPages =(int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResultDto<RoomTypeDto>
        {
            Items = roomTypeDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
    
    public async Task<RoomTypeDto?> GetByIdAsync(int id)
    {
        var roomType = await _roomTypeRepository.GetByIdWithAmenitiesAsync(id);

        if (roomType is null)
            return null;

        return _mapper.Map<RoomTypeDto>(roomType);
    }

    public async Task<IEnumerable<RoomTypeDto>> GetByAmenityIdAsync(int amenityId)
    {
        var amenityExists = await _roomTypeRepository.AmenityExistsAsync(amenityId);

        if (!amenityExists)
        {
            throw new NotFoundException("Amenity not found.");
        }

        var roomTypes =await _roomTypeRepository.GetByAmenityIdAsync(amenityId);

        return _mapper.Map<IEnumerable<RoomTypeDto>>(roomTypes);
    }

    public async Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto dto)
    {
        var exists =await _roomTypeRepository.ExistsByNameAsync(dto.Name);

        if (exists)
        {
            throw new ConflictException("A room type with the same name already exists.");
        }

        var roomType =_mapper.Map<RoomType>(dto);

        await _roomTypeRepository.AddAsync(roomType);

        await _roomTypeRepository.SaveChangesAsync();

        return _mapper.Map<RoomTypeDto>(roomType);
    }

    public async Task<bool> UpdateAsync(int id,UpdateRoomTypeDto dto)
    {
        var roomType =await _roomTypeRepository.GetByIdAsync(id);

        if (roomType is null)
            return false;

        var exists =await _roomTypeRepository.ExistsByNameAsync(dto.Name);

        if (exists &&!roomType.Name.Equals(dto.Name,StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("A room type with the same name already exists.");
        }

        _mapper.Map(dto, roomType);

        _roomTypeRepository.Update(roomType);

        await _roomTypeRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var roomType = await _roomTypeRepository.GetByIdAsync(id);

        if (roomType is null)
            return false;

        _roomTypeRepository.Delete(roomType);

        await _roomTypeRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddAmenityAsync(int roomTypeId,int amenityId)
    {
        var roomTypeExists =await _roomTypeRepository.GetByIdAsync(roomTypeId);

        if (roomTypeExists is null)
        {
            throw new NotFoundException("Room type not found.");
        }

        var amenityExists =await _roomTypeRepository.AmenityExistsAsync(amenityId);

        if (!amenityExists)
        {
            throw new NotFoundException("Amenity not found.");
        }
        var relationExists = await _roomTypeRepository.RoomTypeAmenityExistsAsync(roomTypeId, amenityId);

        if (relationExists)
        {
            throw new ConflictException("This amenity is already assigned to this room type.");
        }

        await _roomTypeRepository.AddAmenityAsync(roomTypeId,amenityId);

        await _roomTypeRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAmenityAsync(int roomTypeId,int amenityId)
    {
        var roomTypeExists =await _roomTypeRepository.GetByIdAsync(roomTypeId);

        if (roomTypeExists is null)
        {
            throw new NotFoundException("Room type not found.");
        }
        var removed =await _roomTypeRepository.RemoveAmenityAsync(roomTypeId,amenityId);

        if (!removed)
            return false;

        await _roomTypeRepository.SaveChangesAsync();

        return true;
    }
}