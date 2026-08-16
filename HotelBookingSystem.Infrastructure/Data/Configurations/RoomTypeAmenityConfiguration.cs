using HotelBookingSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingSystem.Infrastructure.Data.Configurations;

public class RoomTypeAmenityConfiguration
    : IEntityTypeConfiguration<RoomTypeAmenity>
{
    public void Configure(EntityTypeBuilder<RoomTypeAmenity> builder)
    {
        builder.HasKey(x => new
        {
            x.RoomTypeId,
            x.AmenityId
        });

        builder.HasOne(x => x.RoomType)
            .WithMany(x => x.RoomTypeAmenities)
            .HasForeignKey(x => x.RoomTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Amenity)
            .WithMany(x => x.RoomTypeAmenities)
            .HasForeignKey(x => x.AmenityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}