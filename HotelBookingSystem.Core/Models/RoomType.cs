using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Core.Models
{
    public class RoomType
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal PricePerNight { get; set; }

        public int MaxGuests { get; set; }

        public ICollection<Room> Rooms { get; set; } = new List<Room>();

        public ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; }
            = new List<RoomTypeAmenity>();
    }
}
