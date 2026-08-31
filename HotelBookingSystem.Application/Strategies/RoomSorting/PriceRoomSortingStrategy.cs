using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Strategies.RoomSorting;

public class PriceRoomSortingStrategy : IRoomSortingStrategy
{
    public string SortBy => "price";

    public IQueryable<Room> Apply(IQueryable<Room> rooms) =>
        rooms.OrderBy(x => x.RoomType.PricePerNight).ThenBy(x => x.RoomNumber);
}
