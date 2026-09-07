namespace RoomBooking.Api.Dtos;

/// <summary>
/// One booked slot on one day.
/// </summary>
/// <remarks>
public sealed record OccurrenceDto(
    int ReservationId,
    int RoomId,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string BookedBy);
