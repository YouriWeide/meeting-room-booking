using RoomBooking.Api.Dtos;
using RoomBooking.Api.Infrastructure.Repositories;

namespace RoomBooking.Api.Services;

public class RoomService(IRoomRepository rooms) : IRoomService
{
    public async Task<IReadOnlyList<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = await rooms.GetAllAsync(cancellationToken);
        return [.. all.Select(r => new RoomDto(r.Id, r.Name, r.Capacity))];
    }
}
