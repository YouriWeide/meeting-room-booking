namespace RoomBooking.Api.Dtos;

/// <summary>One requested slot that is already taken, and by whom.</summary>
/// <remarks>
/// Carried on the 409 as a <c>conflicts</c> extension so the UI can say "weeks 5 and 7
/// are taken by Sam, book the other 8 or pick another time" rather than just refusing.
/// </remarks>
public sealed record ConflictDto(
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    BlockingBookingDto ConflictsWith);

/// <summary>The existing booking standing in the way.</summary>
public sealed record BlockingBookingDto(
    int ReservationId,
    string BookedBy,
    TimeOnly Start,
    TimeOnly End);
