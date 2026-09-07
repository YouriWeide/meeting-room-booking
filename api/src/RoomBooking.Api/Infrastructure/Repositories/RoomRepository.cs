using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Domain.Entities;

namespace RoomBooking.Api.Infrastructure.Repositories;

public class RoomRepository(BookingDbContext context) : IRoomRepository
{
    public async Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Rooms
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int roomId, CancellationToken cancellationToken = default) =>
        context.Rooms.AnyAsync(r => r.Id == roomId, cancellationToken);
}
