using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Services;

public interface IReservationService
{
    /// <summary>Books a room, once or weekly for a number of weeks.</summary>
    /// <exception cref="Domain.Exceptions.ConflictException">
    /// Thrown when requested slots are already taken and the request did not allow
    /// skipping them, or when skipping would leave nothing to book.
    /// </exception>
    /// <exception cref="Domain.Exceptions.RoomNotFoundException">
    /// Thrown when <c>roomId</c> names a room that does not exist.
    /// </exception>
    Task<ReservationDto> CreateAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default);
}
