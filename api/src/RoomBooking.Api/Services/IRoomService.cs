using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Services;

public interface IRoomService
{
    Task<IReadOnlyList<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
