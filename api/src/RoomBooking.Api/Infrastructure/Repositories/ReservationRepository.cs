using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public void Add(Reservation reservation) => context.Reservations.Add(reservation);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public async Task<IDbContextTransaction> BeginImmediateTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = (SqliteConnection)context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        // deferred: false means BEGIN IMMEDIATE: take the write lock now, not at the
        // first write. Waiting is too late, because by then both callers have already
        // read the slot as free. Handing it to EF keeps SaveChangesAsync inside it.
        var transaction = connection.BeginTransaction(deferred: false);
        return await context.Database.UseTransactionAsync(transaction, cancellationToken)
               ?? throw new InvalidOperationException("Could not enlist the SQLite transaction.");
    }
}
