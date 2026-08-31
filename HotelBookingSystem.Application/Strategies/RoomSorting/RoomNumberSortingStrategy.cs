using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Strategies.RoomSorting;

public class RoomNumberSortingStrategy : IRoomSortingStrategy
{
    public string SortBy => "roomNumber";

    public IQueryable<Room> Apply(IQueryable<Room> rooms) =>
        rooms.OrderBy(x => x.RoomNumber);
}
