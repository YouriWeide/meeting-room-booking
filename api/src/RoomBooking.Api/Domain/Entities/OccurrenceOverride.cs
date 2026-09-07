namespace RoomBooking.Api.Domain.Entities;

/// <summary>
/// A single occurrence that deviates from the series it belongs to.
/// </summary>
/// <remarks>
/// <para>
/// This is how one instance can be cancelled or moved without touching the rest of the
/// series: the reservation row stays exactly as it was, and expansion consults these
/// rows as it walks the generated dates.
/// </para>
/// </remarks>
public class OccurrenceOverride
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public Reservation Reservation { get; set; } = null!;

    public DateOnly OccurrenceDate { get; set; }

    public OverrideKind Kind { get; set; }

    /// <summary>Set only when <see cref="Kind"/> is <see cref="OverrideKind.Moved"/>.</summary>
    public TimeOnly? OverrideStartTime { get; set; }

    /// <summary>Set only when <see cref="Kind"/> is <see cref="OverrideKind.Moved"/>.</summary>
    public TimeOnly? OverrideEndTime { get; set; }
}
