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

        if (conflicts.Count > 0 && !request.SkipConflicts)
        {
            throw new ConflictException(conflicts);
        }

        // Skipping every week would create a reservation that books nothing. Refusing is
        // more honest than returning an empty success the user has to interpret.
        if (conflicts.Count == wanted.Count)
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

    private static ReservationDto ToDto(Reservation reservation) =>
        new(
            reservation.Id,
            reservation.RoomId,
            reservation.BookedBy,
            reservation.FirstDate,
            reservation.LastDate,
            reservation.StartTime,
            reservation.EndTime,
            reservation.OccurrenceCount,
            [.. reservation.Expand().Select(o =>
                new OccurrenceDto(o.ReservationId, o.RoomId, o.Date, o.Start, o.End, o.BookedBy))],
            [.. reservation.Overrides
                .Where(o => o.Kind == OverrideKind.Skipped)
                .Select(o => o.OccurrenceDate)
                .Order()]);
}
