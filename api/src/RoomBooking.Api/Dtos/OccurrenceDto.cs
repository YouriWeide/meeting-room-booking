namespace RoomBooking.Api.Dtos;

/// <summary>
/// One booked slot on one day.
/// </summary>
/// <remarks>
/// Computed from a reservation's recurrence rule, never stored. Every occurrence of a
/// series carries the same <paramref name="ReservationId"/>; the date is what tells them
/// apart, which is why cancelling one is addressed by reservation and date.
/// </remarks>
public sealed record OccurrenceDto(
    int ReservationId,
    int RoomId,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string BookedBy,
    int SeriesOccurrences);
