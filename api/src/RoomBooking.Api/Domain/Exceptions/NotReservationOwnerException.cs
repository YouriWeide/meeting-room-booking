namespace RoomBooking.Api.Domain.Exceptions;

/// <summary>
/// Raised when someone tries to cancel a reservation booked under a different name.
/// </summary>
/// <remarks>
/// A guardrail, not authorisation. The name is client-asserted and trivially spoofable,
/// so this stops honest mistakes and nothing else. Saying that plainly is better than
/// implying the API is protected.
/// </remarks>
public sealed class NotReservationOwnerException(int reservationId)
    : Exception($"Reservation {reservationId} was booked by someone else.");
