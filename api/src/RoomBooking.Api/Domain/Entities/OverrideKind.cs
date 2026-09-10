namespace RoomBooking.Api.Domain.Entities;

/// <summary>Why one occurrence of a series does not happen.</summary>
/// <remarks>
/// Both kinds are dropped from the schedule; they differ only in the reason, which is
/// what lets a booking report which weeks it had to skip.
/// </remarks>
public enum OverrideKind
{
    /// <summary>The user cancelled this one occurrence.</summary>
    Cancelled = 0,

    /// <summary>
    /// Never booked at all: it clashed with an existing reservation when the series was
    /// created and the user chose to go ahead with the rest.
    /// </summary>
    Skipped = 1,
}
