using RoomBooking.Api.Domain.Entities;
using RoomBooking.Api.Domain.Exceptions;
using RoomBooking.Api.Domain.Models;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Infrastructure.Repositories;

namespace RoomBooking.Api.Services;

public class ReservationService(IReservationRepository reservations, IRoomRepository rooms)
    : IReservationService
{
    public async Task<ReservationDto> CreateAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        // [Required] on the request guarantees these are present: the controller never
        // runs with an invalid ModelState.
        var roomId = request.RoomId!.Value;

        if (!await rooms.ExistsAsync(roomId, cancellationToken))
        {
            throw new RoomNotFoundException(roomId);
        }

        var rule = new RecurrenceRule(
            request.Date!.Value, request.StartTime!.Value, request.EndTime!.Value, request.Weeks);

        var reservation = new Reservation
        {
            RoomId = roomId,
            BookedBy = request.BookedBy.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
        };
        reservation.ApplyRecurrence(rule);

        // Everything below happens with SQLite's write lock already held. Two people
        // booking the same room at the same moment serialise here: the second one's
        // conflict check runs against the first one's committed rows, not against the
        // state both of them saw before either wrote.
        await using var transaction = await reservations.BeginImmediateTransactionAsync(cancellationToken);

        var conflicts = await FindConflictsAsync(reservation, rule, cancellationToken);
        var wanted = reservation.Expand().ToList();

        // Refused for either of two reasons: the caller did not ask to skip clashes, or
        // skipping them would leave nothing at all to book. An empty reservation is not
        // a success worth returning.
        var nothingWouldRemain = conflicts.Count == wanted.Count;

        if (conflicts.Count > 0 && (!request.SkipConflicts || nothingWouldRemain))
        {
            throw new ConflictException(conflicts);
        }

        foreach (var conflict in conflicts)
        {
            reservation.SkipOccurrence(conflict.Requested.Date);
        }

        reservations.Add(reservation);
        await reservations.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToDto(reservation);
    }

    public async Task CancelSeriesAsync(
        int reservationId,
        string bookedBy,
        CancellationToken cancellationToken = default)
    {
        var reservation = await LoadOwnedAsync(reservationId, bookedBy, cancellationToken);

        // No transaction: this sets one flag, and setting it twice is the same as
        // setting it once. Nothing is read and then acted on, so there is no race to
        // lose - unlike creating, where the check and the insert must be indivisible.
        reservation.Cancel();
        await reservations.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelOccurrenceAsync(
        int reservationId,
        DateOnly occurrenceDate,
        string bookedBy,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await reservations.BeginImmediateTransactionAsync(cancellationToken);

        var reservation = await LoadOwnedAsync(reservationId, bookedBy, cancellationToken);

        if (!reservation.HasOccurrenceOn(occurrenceDate))
        {
            throw ReservationNotFoundException.Occurrence(reservationId, occurrenceDate);
        }

        reservation.CancelOccurrence(occurrenceDate);
        await reservations.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Reservation> LoadOwnedAsync(
        int reservationId,
        string bookedBy,
        CancellationToken cancellationToken)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, cancellationToken)
                          ?? throw ReservationNotFoundException.Reservation(reservationId);

        if (!string.Equals(reservation.BookedBy, bookedBy.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new NotReservationOwnerException(reservationId);
        }

        return reservation;
    }

    /// <summary>
    /// Which of the requested slots are already taken, and by what.
    /// </summary>
    private async Task<IReadOnlyList<OccurrenceConflict>> FindConflictsAsync(
        Reservation reservation,
        RecurrenceRule rule,
        CancellationToken cancellationToken)
    {
        var existing = (await reservations.GetInRangeAsync(
                rule.FirstDate,
                rule.LastDate,
                reservation.RoomId,
                cancellationToken: cancellationToken))
            .SelectMany(r => r.Expand())
            .ToList();

        return
        [
            .. from wanted in reservation.Expand()
               let blocker = existing.FirstOrDefault(wanted.OverlapsWith)
               where blocker is not null
               select new OccurrenceConflict(wanted, blocker),
        ];
    }

    private static ReservationDto ToDto(Reservation reservation)
    {
        var expanded = reservation.Expand().ToList();
        IReadOnlyList<OccurrenceDto> occurrences = [.. expanded.Select(o => o.ToDto(expanded.Count))];

        return new ReservationDto(
            reservation.Id,
            reservation.RoomId,
            reservation.BookedBy,
            reservation.FirstDate,
            reservation.LastDate,
            reservation.StartTime,
            reservation.EndTime,
            reservation.OccurrenceCount,
            occurrences,
            [.. reservation.Overrides
                .Where(o => o.Kind == OverrideKind.Skipped)
                .Select(o => o.OccurrenceDate)
                .Order()]);
    }
}
