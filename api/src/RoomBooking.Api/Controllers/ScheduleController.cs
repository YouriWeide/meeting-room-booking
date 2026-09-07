using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Services;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/schedule")]
public class ScheduleController(IScheduleService scheduleService) : ControllerBase
{
    /// <summary>
    /// Everything booked between two dates, inclusive, optionally for one room.
    /// </summary>
    /// <param name="from">First day to include, as yyyy-MM-dd.</param>
    /// <param name="to">Last day to include, as yyyy-MM-dd.</param>
    /// <param name="roomId">Limit to one room. Omit for every room.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OccurrenceDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<OccurrenceDto>>> GetSchedule(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? roomId,
        CancellationToken cancellationToken)
    {
        if (to < from)
        {
            ModelState.AddModelError(nameof(to), "'to' must not be earlier than 'from'.");
            return ValidationProblem(ModelState);
        }

        return Ok(await scheduleService.GetAsync(from, to, roomId, cancellationToken));
    }
}
