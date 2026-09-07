using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Domain.Exceptions;
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
        try
        {
            var reservation = await reservationService.CreateAsync(request, cancellationToken);

            return Created((string?)null, reservation);
        }
        catch (RoomNotFoundException exception)
        {
            ModelState.AddModelError(nameof(request.RoomId), exception.Message);
            return ValidationProblem(ModelState);
        }
        catch (ConflictException exception)
        {
            return Conflict(BuildConflictProblem(exception));
        }
    }

    private static ProblemDetails BuildConflictProblem(ConflictException exception)
    {
        var problem = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            Title = "The room is already booked for part of this request.",
            Status = StatusCodes.Status409Conflict,
            Detail = $"{exception.Conflicts.Count} of the requested slots are taken. "
                     + "Send skipConflicts=true to book the remaining weeks, or choose another time.",
        };

        problem.Extensions["conflicts"] = exception.Conflicts
            .Select(c => new ConflictDto(
                c.Requested.Date,
                c.Requested.Start,
                c.Requested.End,
                new BlockingBookingDto(
                    c.ExistingOccurrence.ReservationId,
                    c.ExistingOccurrence.BookedBy,
                    c.ExistingOccurrence.Start,
                    c.ExistingOccurrence.End)))
            .ToList();

        return problem;
    }
}
