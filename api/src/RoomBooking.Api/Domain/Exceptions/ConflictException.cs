using RoomBooking.Api.Domain.Models;

namespace RoomBooking.Api.Domain.Exceptions;

/// <summary>
/// Raised when requested occurrences collide with occurrences that already exist.
/// </summary>
/// <remarks>
/// Has the specific clashes rather than a message, so the API can answer 409 with
/// exactly which weeks are taken.
/// </remarks>
public sealed class ConflictException(IReadOnlyList<OccurrenceConflict> conflicts)
    : Exception($"{conflicts.Count} of the requested occurrences conflict with existing reservations.")
{
    public IReadOnlyList<OccurrenceConflict> Conflicts { get; } = conflicts;
}

/// <summary>A requested occurrence and the existing one that blocks it.</summary>
public sealed record OccurrenceConflict(Occurrence Requested, Occurrence ExistingOccurrence);
