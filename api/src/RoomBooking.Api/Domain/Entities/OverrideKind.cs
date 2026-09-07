namespace RoomBooking.Api.Domain.Entities;

/// <summary>How a single occurrence deviates from the series that generated it.</summary>
public enum OverrideKind
{
    /// <summary>The user cancelled this one occurrence.</summary>
    Cancelled = 0,

    /// <summary>
    /// Never booked at all: it conflicted with an existing reservation when the series
    /// was created and the user chose to skip it.
    /// </summary>
    Skipped = 1,

    /// <summary>Rescheduled to a different time on the same day.</summary>
    Moved = 2,
}
