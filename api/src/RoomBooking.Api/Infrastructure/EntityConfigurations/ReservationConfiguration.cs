using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.Api.Domain.Entities;

namespace RoomBooking.Api.Infrastructure.EntityConfigurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.Property(r => r.BookedBy)
            .HasMaxLength(100);

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Overrides)
            .WithOne(o => o.Reservation)
            .HasForeignKey(o => o.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.RoomId, r.FirstDate, r.LastDate });
    }
}
