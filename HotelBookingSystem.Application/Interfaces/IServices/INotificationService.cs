using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Application.Interfaces.IServices;

public interface INotificationService
{
    Task SendAsync(string recipient, string subject, string message);
}