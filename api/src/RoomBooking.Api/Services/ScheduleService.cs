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
        var candidates = await reservations.GetInRangeAsync(rangeStart, rangeEnd, roomId, cancellationToken: cancellationToken);

        return
        [
            .. candidates
                .SelectMany(r => r.Expand())
                // The repository matched whole reservations whose span touches the range,
                // so a ten-week series shows up here with all ten of its occurrences even
                // when only one falls inside the week being viewed.
                .Where(o => o.Date >= rangeStart && o.Date <= rangeEnd)
                .OrderBy(o => o.Date)
                .ThenBy(o => o.Start)
                .ThenBy(o => o.RoomId)
                .Select(o => new OccurrenceDto(o.ReservationId, o.RoomId, o.Date, o.Start, o.End, o.BookedBy)),
        ];
    }
}
