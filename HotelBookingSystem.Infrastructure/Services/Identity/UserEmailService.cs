using HotelBookingSystem.Application.Interfaces.IServices;
using HotelBookingSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HotelBookingSystem.Infrastructure.Services.Identity;

public class UserEmailService : IUserEmailService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserEmailService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> GetEmailByUserIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.Email;
    }
}
