using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Core.Models
{
    public class Amenity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; }
            = new List<RoomTypeAmenity>();
    }
}