namespace RoomBooking.Api.Domain.Exceptions;

/// <summary>Raised when a reservation, or one of its occurrences, does not exist.</summary>
public sealed class ReservationNotFoundException(string message) : Exception(message)
{
    public static ReservationNotFoundException Reservation(int id) =>
        new($"Reservation {id} does not exist.");

    public static ReservationNotFoundException Occurrence(int id, DateOnly date) =>
        new($"Reservation {id} has no occurrence on {date:yyyy-MM-dd}.");
}
