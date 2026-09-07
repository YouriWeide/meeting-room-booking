namespace RoomBooking.Api.Dtos;

/// <summary>A created reservation and the slots it actually claimed.</summary>
/// <param name="Occurrences">The slots that were booked.</param>
/// <param name="SkippedDates">
/// Weeks that were requested but were already taken, and were skipped because the
/// request allowed it. Empty on a clean booking.
/// </param>
public sealed record ReservationDto(
    int Id,
    int RoomId,
    string BookedBy,
    DateOnly FirstDate,
    DateOnly LastDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Weeks,
    IReadOnlyList<OccurrenceDto> Occurrences,
    IReadOnlyList<DateOnly> SkippedDates);
