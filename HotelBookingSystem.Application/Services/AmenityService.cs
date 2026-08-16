using AutoMapper;
using HotelBookingSystem.Application.DTOs.Amenities;
using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.Exceptions;
using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Application.Interfaces.IServices;
using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Services;

public class AmenityService : IAmenityService
{
    private readonly IAmenityRepository _amenityRepository;
    private readonly IMapper _mapper;

    public AmenityService(IAmenityRepository amenityRepository,IMapper mapper)
    {
        _amenityRepository = amenityRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<AmenityDto>> GetAllAsync(int pageNumber,int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var result =await _amenityRepository.GetPagedAsync(pageNumber,pageSize);

        var items = result.Items;

        var totalCount = result.TotalCount;

        var amenityDtos =_mapper.Map<IEnumerable<AmenityDto>>(items);

        var totalPages =(int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResultDto<AmenityDto>
        {
            Items = amenityDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
    public async Task<AmenityDto?> GetByIdAsync(int id)
    {
        var amenity =await _amenityRepository.GetByIdAsync(id);

        if (amenity is null)
            return null;

        return _mapper.Map<AmenityDto>(amenity);
    }

    public async Task<AmenityDto> CreateAsync(CreateAmenityDto dto)
    {
        var exists = await _amenityRepository.ExistsByNameAsync(dto.Name);

            if (exists)
            {
                throw new ConflictException(
                    "An amenity with the same name already exists.");
            }
        
        var amenity =_mapper.Map<Amenity>(dto);

        await _amenityRepository.AddAsync(amenity);

        await _amenityRepository.SaveChangesAsync();

        return _mapper.Map<AmenityDto>(amenity);
    }

    public async Task<bool> UpdateAsync(int id,UpdateAmenityDto dto)
    {
        var amenity = await _amenityRepository.GetByIdAsync(id);

        if (amenity is null)
            throw new NotFoundException("Amenity not found.");

        var exists = await _amenityRepository.ExistsByNameAsync(dto.Name);

        if (exists &&!amenity.Name.Equals(dto.Name,StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("An amenity with the same name already exists.");
        }

        _mapper.Map(dto, amenity);

        _amenityRepository.Update(amenity);

        await _amenityRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var amenity = await _amenityRepository.GetByIdAsync(id);

        if (amenity is null)
            throw new NotFoundException("Amenity not found.");

        _amenityRepository.Delete(amenity);

        await _amenityRepository.SaveChangesAsync();

        return true;
    }
}