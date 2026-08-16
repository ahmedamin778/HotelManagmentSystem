using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Core.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        public Booking Booking { get; set; } = null!;

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
