using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Strategies.RoomSorting;

public class NameRoomSortingStrategy : IRoomSortingStrategy
{
    public string SortBy => "name";

    public IQueryable<Room> Apply(IQueryable<Room> rooms) =>
        rooms.OrderBy(x => x.RoomType.Name).ThenBy(x => x.RoomNumber);
}
