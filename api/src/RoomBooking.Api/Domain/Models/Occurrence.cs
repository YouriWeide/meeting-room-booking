namespace RoomBooking.Api.Domain.Models;

/// <summary>
/// One concrete booked slot from a recurrence.
/// </summary>
/// <remarks>
/// Computed, never persisted. Occurrences are derived by expanding a reservation's
/// recurrence rule and applying its exceptions.
/// </remarks>
public sealed record Occurrence(
    int ReservationId,
    int RoomId,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string BookedBy)
{
    /// <summary>
    /// Whether two occurrences collide.
    /// </summary>
    /// <remarks>
    /// Half-open intervals: [Start, End). A room is free again at its end time, so
    /// 09:00-10:00 and 10:00-11:00 do not overlap. Occurrences on different days or in
    /// different rooms can never collide.
    /// </remarks>
    public bool OverlapsWith(Occurrence other) =>
        RoomId == other.RoomId
        && Date == other.Date
        && Start < other.End
        && other.Start < End;
}
