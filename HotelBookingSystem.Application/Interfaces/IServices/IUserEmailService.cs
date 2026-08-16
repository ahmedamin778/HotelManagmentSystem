namespace HotelBookingSystem.Application.Interfaces.IServices;

/// <summary>
/// Provides the email address for an application user without exposing
/// the Identity implementation to application services.
/// </summary>
public interface IUserEmailService
{
    Task<string?> GetEmailByUserIdAsync(string userId);
}
