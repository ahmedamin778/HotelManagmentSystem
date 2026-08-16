using HotelBookingSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingSystem.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Review and Booking
        builder.HasOne(x => x.Booking)
            .WithOne(x => x.Review)
            .HasForeignKey<Review>(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // One booking can have only one review
        builder.HasIndex(x => x.BookingId)
            .IsUnique();
    }
}