using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Application.DTOs.Rooms
{
    public class RoomDto
    {
        public int Id { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public int RoomTypeId { get; set; }

        public string RoomTypeName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public byte[] RowVersion { get; set; } = [];
    }
}
