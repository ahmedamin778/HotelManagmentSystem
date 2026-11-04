using HotelBookingSystem.Core.Enums;
using HotelBookingSystem.Core.Models;
using HotelBookingSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBookingSystem.Infrastructure.Data;

public static class DatabaseSeeder
{
    private const string AdminEmail = "admin@hotel.com";
    private const string StaffEmail = "staff@hotel.com";
    private const string GuestEmail = "guest@hotel.com";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        await EnsureRolesAsync(roleManager);

        await EnsureUserAsync(
            userManager,
            "Hotel Admin",
            AdminEmail,
            "Admin@123456",
            "Admin");

        await EnsureUserAsync(
            userManager,
            "Mariam Hassan",
            StaffEmail,
            "Staff@123456",
            "Staff");

        var guest = await EnsureUserAsync(
            userManager,
            "Omar Khaled",
            GuestEmail,
            "Guest@123456",
            "Guest");

        var amenities = await SeedAmenitiesAsync(context);
        var roomTypes = await SeedRoomTypesAsync(context);

        await SeedRoomTypeAmenitiesAsync(context, roomTypes, amenities);

        var rooms = await SeedRoomsAsync(context, roomTypes);

        await SeedBookingsAndReviewsAsync(context, guest.Id, rooms);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Staff", "Guest"];

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            EnsureIdentitySucceeded(result);
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string fullName,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                FullName = fullName,
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            EnsureIdentitySucceeded(createResult);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            EnsureIdentitySucceeded(roleResult);
        }

        return user;
    }

    private static async Task<Dictionary<string, Amenity>> SeedAmenitiesAsync(
        ApplicationDbContext context)
    {
        string[] names = ["Free Wi-Fi", "Breakfast", "Swimming Pool"];

        var amenities = await context.Amenities
            .Where(x => names.Contains(x.Name))
            .ToDictionaryAsync(x => x.Name);

        foreach (var name in names)
        {
            if (amenities.ContainsKey(name))
                continue;

            var amenity = new Amenity { Name = name };
            context.Amenities.Add(amenity);
            amenities[name] = amenity;
        }

        await context.SaveChangesAsync();
        return amenities;
    }

    private static async Task<Dictionary<string, RoomType>> SeedRoomTypesAsync(
        ApplicationDbContext context)
    {
        var definitions = new[]
        {
            new RoomType { Name = "Standard", PricePerNight = 1200m, MaxGuests = 2 },
            new RoomType { Name = "Deluxe", PricePerNight = 2000m, MaxGuests = 3 },
            new RoomType { Name = "Executive Suite", PricePerNight = 3500m, MaxGuests = 4 }
        };

        var names = definitions.Select(x => x.Name).ToArray();
        var roomTypes = await context.RoomTypes
            .Where(x => names.Contains(x.Name))
            .ToDictionaryAsync(x => x.Name);

        foreach (var definition in definitions)
        {
            if (roomTypes.ContainsKey(definition.Name))
                continue;

            context.RoomTypes.Add(definition);
            roomTypes[definition.Name] = definition;
        }

        await context.SaveChangesAsync();
        return roomTypes;
    }

    private static async Task SeedRoomTypeAmenitiesAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, RoomType> roomTypes,
        IReadOnlyDictionary<string, Amenity> amenities)
    {
        var relations = new[]
        {
            (RoomType: "Standard", Amenity: "Free Wi-Fi"),
            (RoomType: "Deluxe", Amenity: "Free Wi-Fi"),
            (RoomType: "Deluxe", Amenity: "Breakfast"),
            (RoomType: "Executive Suite", Amenity: "Free Wi-Fi"),
            (RoomType: "Executive Suite", Amenity: "Breakfast"),
            (RoomType: "Executive Suite", Amenity: "Swimming Pool")
        };

        foreach (var relation in relations)
        {
            var roomTypeId = roomTypes[relation.RoomType].Id;
            var amenityId = amenities[relation.Amenity].Id;

            var exists = await context.RoomTypeAmenities.AnyAsync(x =>
                x.RoomTypeId == roomTypeId && x.AmenityId == amenityId);

            if (!exists)
            {
                context.RoomTypeAmenities.Add(new RoomTypeAmenity
                {
                    RoomTypeId = roomTypeId,
                    AmenityId = amenityId
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, Room>> SeedRoomsAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, RoomType> roomTypes)
    {
        var definitions = new[]
        {
            new Room { RoomNumber = "101", RoomTypeId = roomTypes["Standard"].Id, RoomType = roomTypes["Standard"], IsActive = true },
            new Room { RoomNumber = "102", RoomTypeId = roomTypes["Standard"].Id, RoomType = roomTypes["Standard"], IsActive = true },
            new Room { RoomNumber = "201", RoomTypeId = roomTypes["Deluxe"].Id, RoomType = roomTypes["Deluxe"], IsActive = true },
            new Room { RoomNumber = "301", RoomTypeId = roomTypes["Executive Suite"].Id, RoomType = roomTypes["Executive Suite"], IsActive = true }
        };

        var roomNumbers = definitions.Select(x => x.RoomNumber).ToArray();
        var rooms = await context.Rooms
            .Include(x => x.RoomType)
            .Where(x => roomNumbers.Contains(x.RoomNumber))
            .ToDictionaryAsync(x => x.RoomNumber);

        foreach (var definition in definitions)
        {
            if (rooms.ContainsKey(definition.RoomNumber))
                continue;

            context.Rooms.Add(definition);
            rooms[definition.RoomNumber] = definition;
        }

        await context.SaveChangesAsync();
        return rooms;
    }

    private static async Task SeedBookingsAndReviewsAsync(
        ApplicationDbContext context,
        string guestId,
        IReadOnlyDictionary<string, Room> rooms)
    {
        var today = DateTime.UtcNow.Date;
        var definitions = new[]
        {
            CreateBooking(rooms["101"], guestId, today.AddDays(14), today.AddDays(17), BookingStatus.Requested, today.AddDays(-1)),
            CreateBooking(rooms["201"], guestId, today.AddDays(30), today.AddDays(34), BookingStatus.Confirmed, today.AddDays(-3)),
            CreateBooking(rooms["301"], guestId, today.AddDays(-1), today.AddDays(2), BookingStatus.CheckedIn, today.AddDays(-10)),
            CreateBooking(rooms["102"], guestId, today.AddDays(-20), today.AddDays(-17), BookingStatus.CheckedOut, today.AddDays(-30))
        };

        foreach (var definition in definitions)
        {
            var exists = await context.Bookings.AnyAsync(x =>
                x.GuestId == guestId && x.RoomId == definition.RoomId);

            if (!exists)
                context.Bookings.Add(definition);
        }

        await context.SaveChangesAsync();

        var completedBooking = await context.Bookings
            .FirstAsync(x => x.GuestId == guestId && x.RoomId == rooms["102"].Id);

        if (!await context.Reviews.AnyAsync(x => x.BookingId == completedBooking.Id))
        {
            context.Reviews.Add(new Review
            {
                BookingId = completedBooking.Id,
                Rating = 5,
                Comment = "Excellent stay, spotless room and very helpful staff.",
                CreatedAt = today.AddDays(-16)
            });

            await context.SaveChangesAsync();
        }
    }

    private static Booking CreateBooking(
        Room room,
        string guestId,
        DateTime checkIn,
        DateTime checkOut,
        BookingStatus status,
        DateTime createdAt)
    {
        var numberOfNights = (checkOut - checkIn).Days;

        return new Booking
        {
            GuestId = guestId,
            RoomId = room.Id,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            NumberOfNights = numberOfNights,
            TotalPrice = numberOfNights * room.RoomType.PricePerNight,
            Status = status,
            CreatedAt = createdAt
        };
    }

    private static void EnsureIdentitySucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(x => x.Description)));
        }
    }
}
