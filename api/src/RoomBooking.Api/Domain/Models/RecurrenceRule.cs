namespace RoomBooking.Api.Domain.Models;

/// <summary>
/// The "every week at this time, N times" rule behind a reservation.
/// </summary>
/// <remarks>
/// A single booking is a rule with <see cref="OccurrenceCount"/> of 1. Modelling it
/// that way is deliberate: creating, conflict-checking and rendering never have to
/// branch on "is this recurring?", because there is only ever a series.
/// </remarks>
public readonly record struct RecurrenceRule(
    DateOnly FirstDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int OccurrenceCount)
{
    /// <summary>
    /// Weekly is the only interval this system offers, so the step is a constant rather
    /// than a field. An interval column that is always 1 would be a promise the product
    /// does not make; adding one later is a smaller change than explaining a dead field.
    /// </summary>
    private const int DaysBetweenOccurrences = 7;

    /// <summary>The date of the final occurrence.</summary>
    /// <remarks>
    /// Persisted on the reservation as well, so the conflict check can find candidate
    /// reservations with an indexed range query instead of expanding every row.
    /// </remarks>
    public DateOnly LastDate => FirstDate.AddDays((OccurrenceCount - 1) * DaysBetweenOccurrences);

    /// <summary>Every date this rule generates, in order.</summary>
    public IEnumerable<DateOnly> Dates()
    {
        for (var i = 0; i < OccurrenceCount; i++)
        {
            yield return FirstDate.AddDays(i * DaysBetweenOccurrences);
        }
    }
}
