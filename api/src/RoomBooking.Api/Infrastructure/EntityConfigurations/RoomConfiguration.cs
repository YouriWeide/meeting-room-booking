using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.Api.Domain.Entities;

namespace RoomBooking.Api.Infrastructure.EntityConfigurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.Property(r => r.Name)
            .HasMaxLength(100);

        builder.HasIndex(r => r.Name).IsUnique();
    }
}
