using RoomBooking.Api.Domain.Entities;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Infrastructure.Repositories;

namespace RoomBooking.Api.Services;

public class ScheduleService(IReservationRepository reservations) : IScheduleService
{
    public async Task<IReadOnlyList<OccurrenceDto>> GetAsync(
        DateOnly rangeStart,
        DateOnly rangeEnd,
        int? roomId = null,
        CancellationToken cancellationToken = default)
    {
        var candidates = await reservations.GetInRangeAsync(
            rangeStart, rangeEnd, roomId, cancellationToken: cancellationToken);

        return
        [
            .. candidates
                .SelectMany(reservation => Visible(reservation, rangeStart, rangeEnd))
                .OrderBy(o => o.Date)
                .ThenBy(o => o.Start)
                .ThenBy(o => o.RoomId),
        ];
    }

    private static IEnumerable<OccurrenceDto> Visible(
        Reservation reservation,
        DateOnly rangeStart,
        DateOnly rangeEnd)
    {
        // Expanded once and kept, because the two uses need different things from it:
        // the count describes the whole series, while the rows describe the range.
        var all = reservation.Expand().ToList();

        // The repository matched whole reservations whose span touches the range, so a
        // ten-week series arrives with all ten occurrences even when only one falls in
        // the week being viewed. Without this filter the grid would show the other nine.
        return all
            .Where(o => o.Date >= rangeStart && o.Date <= rangeEnd)
            .Select(o => o.ToDto(all.Count));
    }
}
