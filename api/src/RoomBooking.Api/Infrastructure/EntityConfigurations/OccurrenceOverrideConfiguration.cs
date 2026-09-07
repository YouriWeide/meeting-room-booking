using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.Api.Domain.Entities;

namespace RoomBooking.Api.Infrastructure.EntityConfigurations;

public class OccurrenceOverrideConfiguration : IEntityTypeConfiguration<OccurrenceOverride>
{
    public void Configure(EntityTypeBuilder<OccurrenceOverride> builder)
    {
        builder.Property(o => o.Kind)
            .HasConversion<string>()
            .HasMaxLength(20);


        builder.HasIndex(o => new { o.ReservationId, o.OccurrenceDate }).IsUnique();
    }
}
