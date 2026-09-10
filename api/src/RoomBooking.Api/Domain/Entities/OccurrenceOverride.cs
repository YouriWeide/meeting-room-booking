namespace RoomBooking.Api.Domain.Entities;

/// <summary>
/// One occurrence of a series that does not happen.
/// </summary>
/// <remarks>
/// This is how a single occurrence can be cancelled without touching the rest: the
/// reservation row stays exactly as it was, and expansion consults these rows as it
/// walks the generated dates.
/// </remarks>
public class OccurrenceOverride
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public Reservation Reservation { get; set; } = null!;

    public DateOnly OccurrenceDate { get; set; }

    public OverrideKind Kind { get; set; }
}
