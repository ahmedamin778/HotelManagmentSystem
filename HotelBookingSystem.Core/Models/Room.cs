using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Core.Models
{
    public class Room
    {
        public int Id { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public int RoomTypeId { get; set; }

        public RoomType RoomType { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
