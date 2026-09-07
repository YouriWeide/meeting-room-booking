using Microsoft.EntityFrameworkCore.Storage;
using RoomBooking.Api.Domain.Entities;

namespace RoomBooking.Api.Infrastructure.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reservations whose occurrences could possibly fall inside the given date range,
    /// narrowed by the persisted [FirstDate, LastDate] span.
    /// </summary>
    /// <param name="roomId">Limit to one room, or null for every room.</param>
    /// <param name="excludeReservationId">
    /// Leave this reservation out of the results.
    /// <para>
    /// Only needed when editing an existing series. The row being edited is still in the
    /// database with its old times, so a conflict check for its new times would find it
    /// and report the series as clashing with itself.
    /// </para>
    /// </param>
    /// <remarks>
    /// Returns candidates, not answers: the span overlapping the range does not mean any
    /// individual occurrence does.
    /// </remarks>
    Task<IReadOnlyList<Reservation>> GetInRangeAsync(
        DateOnly rangeStart,
        DateOnly rangeEnd,
        int? roomId = null,
        int? excludeReservationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Stages a new reservation for insert. Nothing is written until it is committed.</summary>
    void Add(Reservation reservation);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a transaction that takes SQLite's single write lock immediately, rather
    /// than at the first write.
    /// </summary>
    Task<IDbContextTransaction> BeginImmediateTransactionAsync(CancellationToken cancellationToken = default);
}
