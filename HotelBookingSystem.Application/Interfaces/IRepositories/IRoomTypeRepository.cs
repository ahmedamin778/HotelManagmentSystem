using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Interfaces.IRepositories;

public interface IRoomTypeRepository : IGenericRepository<RoomType>
{
    Task<bool> ExistsByNameAsync(string name);

    Task<IEnumerable<RoomType>> GetAllWithAmenitiesAsync();

    Task<RoomType?> GetByIdWithAmenitiesAsync(int id);

    Task<IEnumerable<RoomType>> GetByAmenityIdAsync(int amenityId);

    Task<bool> AmenityExistsAsync(int amenityId);

    Task<bool> RoomTypeAmenityExistsAsync(int roomTypeId, int amenityId);

    Task AddAmenityAsync(int roomTypeId, int amenityId);

    Task<bool> RemoveAmenityAsync(int roomTypeId, int amenityId);
    Task<(IEnumerable<RoomType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}