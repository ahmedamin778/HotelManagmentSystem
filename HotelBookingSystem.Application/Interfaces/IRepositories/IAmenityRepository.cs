using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Interfaces.IRepositories;

public interface IAmenityRepository: IGenericRepository<Amenity>
{
    Task<bool> ExistsByNameAsync(string name);
    Task<(IEnumerable<Amenity> Items, int TotalCount)> GetPagedAsync(int pageNumber,int pageSize);
}