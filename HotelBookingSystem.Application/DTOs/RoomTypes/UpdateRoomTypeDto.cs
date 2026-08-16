using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HotelBookingSystem.Application.DTOs.RoomTypes
{
    public class UpdateRoomTypeDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 100000)]
        public decimal PricePerNight { get; set; }

        [Range(1, 20)]
        public int MaxGuests { get; set; }
    }
}
