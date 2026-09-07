using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Domain.Entities;

namespace RoomBooking.Api.Infrastructure.Repositories;

public class ReservationRepository(BookingDbContext context) : IReservationRepository
{
    public Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Reservations
            .Include(r => r.Overrides)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetInRangeAsync(
        DateOnly rangeStart,
        DateOnly rangeEnd,
        int? roomId = null,
        int? excludeReservationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Reservations
            .Include(r => r.Overrides)
            .Where(r => !r.IsCancelled)
            // Keep reservations whose date range overlaps the one we asked about.
            .Where(r => r.FirstDate <= rangeEnd && rangeStart <= r.LastDate);

        if (roomId is not null)
        {
            query = query.Where(r => r.RoomId == roomId);
        }

        if (excludeReservationId is not null)
        {
            query = query.Where(r => r.Id != excludeReservationId);
        }

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public void Add(Reservation reservation) => context.Reservations.Add(reservation);
}
