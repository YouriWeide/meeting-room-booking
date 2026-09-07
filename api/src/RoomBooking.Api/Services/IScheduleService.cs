using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Services;

public interface IScheduleService
{
    /// <summary>
    /// Every booked slot in the given date range.
    /// </summary>
    Task<IReadOnlyList<OccurrenceDto>> GetAsync(
        DateOnly rangeStart,
        DateOnly rangeEnd,
        int? roomId = null,
        CancellationToken cancellationToken = default);
}
