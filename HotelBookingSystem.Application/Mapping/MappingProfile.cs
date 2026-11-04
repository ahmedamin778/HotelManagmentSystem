using AutoMapper;
using HotelBookingSystem.Application.DTOs.Amenities;
using HotelBookingSystem.Application.DTOs.Bookings;
using HotelBookingSystem.Application.DTOs.Reviews;
using HotelBookingSystem.Application.DTOs.Rooms;
using HotelBookingSystem.Application.DTOs.RoomTypes;
using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // =========================
        // RoomType
        // =========================

        CreateMap<RoomType, RoomTypeDto>()
            .ForMember(
                dest => dest.Amenities,
                opt => opt.MapFrom(
                    src => src.RoomTypeAmenities
                        .Select(x => x.Amenity)));

        CreateMap<CreateRoomTypeDto, RoomType>();

        CreateMap<UpdateRoomTypeDto, RoomType>();


        // =========================
        // Room
        // =========================

        CreateMap<Room, RoomDto>()
            .ForMember(
                dest => dest.RoomTypeName,
                opt => opt.MapFrom(
                    src => src.RoomType.Name));

        CreateMap<CreateRoomDto, Room>();

        CreateMap<UpdateRoomDto, Room>()
            .ForMember(
                dest => dest.RowVersion,
                opt => opt.Ignore());


        // =========================
        // Amenity
        // =========================

        CreateMap<Amenity, AmenityDto>();

        CreateMap<CreateAmenityDto, Amenity>();

        CreateMap<UpdateAmenityDto, Amenity>();


        // =========================
        // Booking
        // =========================

        CreateMap<Booking, BookingDto>()
            .ForMember(
                dest => dest.RoomNumber,
                opt => opt.MapFrom(
                    src => src.Room.RoomNumber))

            .ForMember(
                dest => dest.RoomTypeName,
                opt => opt.MapFrom(
                    src => src.Room.RoomType.Name))

            .ForMember(
                dest => dest.PricePerNight,
                opt => opt.MapFrom(
                    src => src.Room.RoomType.PricePerNight))

            .ForMember(
                dest => dest.Status,
                opt => opt.MapFrom(
                    src => src.Status.ToString()));

        // =========================
        // Review
        // =========================

        CreateMap<Review, ReviewDto>()
            .ForMember(
                dest => dest.RoomNumber,
                opt => opt.MapFrom(
                    src => src.Booking.Room.RoomNumber))
            .ForMember(
                dest => dest.RoomTypeName,
                opt => opt.MapFrom(
                    src => src.Booking.Room.RoomType.Name));
    }
}
