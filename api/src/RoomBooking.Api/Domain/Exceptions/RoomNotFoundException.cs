namespace RoomBooking.Api.Domain.Exceptions;

/// <summary>Raised when a booking names a room that does not exist.</summary>
public sealed class RoomNotFoundException(int roomId)
    : Exception($"Room {roomId} does not exist.")
{
    public int RoomId { get; } = roomId;
}
