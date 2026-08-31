using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Strategies.RoomSorting;

public interface IRoomSortingStrategy
{
    string SortBy { get; }
    IQueryable<Room> Apply(IQueryable<Room> rooms);
}
