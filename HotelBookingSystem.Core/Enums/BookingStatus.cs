using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Core.Enums
{
    public enum BookingStatus
    {
        Requested=1,
        Confirmed,
        CheckedIn,
        CheckedOut,
        Cancelled
    }
}
