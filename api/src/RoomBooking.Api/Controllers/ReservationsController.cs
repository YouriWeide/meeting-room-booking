using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Services;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController(IReservationService reservationService) : ControllerBase
{
    /// <summary>Books a room for a time slot, once or weekly for a number of weeks.</summary>
    [HttpPost]
    [ProducesResponseType<ReservationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> CreateReservation(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var reservation = await reservationService.CreateAsync(request, cancellationToken);

        return Created((string?)null, reservation);
    }

    /// <summary>Cancels a whole series, or a single booking.</summary>
    /// <param name="bookedBy">
    /// The name it was booked under.
    /// </param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSeries(
        int id,
        [FromQuery][Required] string bookedBy,
        CancellationToken cancellationToken)
    {
        await reservationService.CancelSeriesAsync(id, bookedBy, cancellationToken);
        return NoContent();
    }

    /// <summary>Cancels one occurrence and leaves the rest of the series untouched.</summary>
    /// <param name="occurrenceDate">Which occurrence, as yyyy-MM-dd.</param>
    [HttpDelete("{id:int}/occurrences/{occurrenceDate}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOccurrence(
        int id,
        DateOnly occurrenceDate,
        [FromQuery][Required] string bookedBy,
        CancellationToken cancellationToken)
    {
        await reservationService.CancelOccurrenceAsync(id, occurrenceDate, bookedBy, cancellationToken);
        return NoContent();
    }
}
