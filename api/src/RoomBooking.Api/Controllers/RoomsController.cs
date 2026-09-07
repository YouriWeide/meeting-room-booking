using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Services;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(IRoomService roomService) : ControllerBase
{
    /// <summary>The rooms that can be booked.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RoomDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> GetRooms(CancellationToken cancellationToken) =>
        Ok(await roomService.GetAllAsync(cancellationToken));
}
