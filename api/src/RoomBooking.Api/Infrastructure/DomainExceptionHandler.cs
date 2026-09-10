using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Domain.Exceptions;
using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Infrastructure;

/// <summary>
/// Turns domain exceptions into RFC 9457 Problem Details.
/// </summary>
/// <remarks>
/// One place rather than a try/catch in every write action. The controllers say what
/// they do; what a failure looks like on the wire is decided here, so a new endpoint
/// cannot accidentally answer the same failure with a different shape.
/// </remarks>
public sealed class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ConflictException conflict => Conflict(conflict),
            RoomNotFoundException room => BadRequest(nameof(room.RoomId), room.Message),
            ReservationNotFoundException notFound => Simple(StatusCodes.Status404NotFound, notFound.Message),
            NotReservationOwnerException owner => Simple(StatusCodes.Status403Forbidden, owner.Message),

            // Anything else is a bug, not a rule. Let it through so it is logged and
            // surfaces as a 500 rather than being quietly dressed up as a client error.
            _ => null,
        };

        if (problem is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static ProblemDetails Simple(int status, string detail) =>
        new() { Status = status, Title = ReasonPhrase(status), Detail = detail };

    private static ProblemDetails BadRequest(string field, string message)
    {
        var problem = Simple(StatusCodes.Status400BadRequest, message);

        // Same shape model validation produces, so a client has one way to read field
        // errors rather than two.
        problem.Extensions["errors"] = new Dictionary<string, string[]> { [field] = [message] };
        return problem;
    }

    private static ProblemDetails Conflict(ConflictException exception)
    {
        var problem = Simple(
            StatusCodes.Status409Conflict,
            $"{exception.Conflicts.Count} of the requested slots are already taken. "
            + "If any remain free, send skipConflicts=true to book those.");

        problem.Title = "The room is already booked for part of this request.";

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

    private static string ReasonPhrase(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "The request is not a valid booking.",
        StatusCodes.Status403Forbidden => "That reservation belongs to someone else.",
        StatusCodes.Status404NotFound => "Not found.",
        StatusCodes.Status409Conflict => "Already booked.",
        _ => "Request failed.",
    };
}
