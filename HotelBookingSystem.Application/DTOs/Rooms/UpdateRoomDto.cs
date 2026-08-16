using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HotelBookingSystem.Application.DTOs.Rooms
{
    public class UpdateRoomDto
    {
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string RoomNumber { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int RoomTypeId { get; set; }
    }
}
