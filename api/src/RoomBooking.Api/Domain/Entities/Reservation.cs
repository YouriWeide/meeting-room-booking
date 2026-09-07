using RoomBooking.Api.Domain.Models;

namespace RoomBooking.Api.Domain.Entities;

/// <summary>
/// One booking: a room, a person, a time range and a weekly recurrence rule.
/// </summary>
public class Reservation
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    public required string BookedBy { get; set; }

    public DateOnly FirstDate { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    /// <summary>Number of occurrences. 1 for a single booking.</summary>
    public int OccurrenceCount { get; private set; } = 1;

    /// <summary>
    /// Date of the last occurrence. Derived from the rule, but persisted so the
    /// conflict check can narrow candidates with an index on (RoomId, FirstDate,
    /// LastDate) rather than expanding every reservation in the room.
    /// </summary>
    /// <remarks>
    /// Kept in sync by <see cref="ApplyRecurrence"/>, which is the only way to change any
    /// part of the rule — every field it derives from has a private setter too. Guarding
    /// the derived value alone would not have been enough: setting OccurrenceCount
    /// directly would leave this stale, and a stale LastDate silently narrows the conflict
    /// check's candidate query, which is a double booking that reports no error.
    /// </remarks>
    public DateOnly LastDate { get; private set; }

    /// <summary>Whether the whole series has been cancelled.</summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Audit timestamp, in UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    public List<OccurrenceOverride> Overrides { get; set; } = [];

    public RecurrenceRule Recurrence =>
        new(FirstDate, StartTime, EndTime, OccurrenceCount);

    /// <summary>
    /// Sets the recurrence and keeps the derived <see cref="LastDate"/> consistent.
    /// </summary>
    public void ApplyRecurrence(RecurrenceRule rule)
    {
        FirstDate = rule.FirstDate;
        StartTime = rule.StartTime;
        EndTime = rule.EndTime;
        OccurrenceCount = rule.OccurrenceCount;
        LastDate = rule.LastDate;
    }

    /// <summary>Cancels the whole series. Expansion then yields nothing.</summary>
    public void Cancel() => IsCancelled = true;

    /// <summary>Whether the rule actually generates an occurrence on this date.</summary>
    public bool HasOccurrenceOn(DateOnly date) => Recurrence.Dates().Contains(date);

    /// <summary>
    /// Cancels one occurrence, leaving the rest of the series exactly as it was.
    /// </summary>
    /// <remarks>
    /// If the occurrence had been moved, the move is replaced rather than kept beside a
    /// cancellation: one date carries at most one override, which the unique index on
    /// (ReservationId, OccurrenceDate) enforces.
    /// </remarks>
    public void CancelOccurrence(DateOnly date)
    {
        var existing = Overrides.FirstOrDefault(o => o.OccurrenceDate == date);

        if (existing is null)
        {
            Overrides.Add(new OccurrenceOverride
            {
                OccurrenceDate = date,
                Kind = OverrideKind.Cancelled,
            });
            return;
        }

        existing.Kind = OverrideKind.Cancelled;
        existing.OverrideStartTime = null;
        existing.OverrideEndTime = null;
    }

    /// <summary>
    /// Records that one occurrence was never booked because it clashed with an existing
    /// reservation and the user chose to go ahead with the rest of the series.
    /// </summary>
    public void SkipOccurrence(DateOnly date) =>
        Overrides.Add(new OccurrenceOverride
        {
            OccurrenceDate = date,
            Kind = OverrideKind.Skipped,
        });

    /// <summary>
    /// Expands this reservation into the occurrences it actually stands for, applying
    /// its overrides. A cancelled series expands to nothing.
    /// </summary>
    public IEnumerable<Occurrence> Expand()
    {
        if (IsCancelled)
        {
            yield break;
        }

        var overridesByDate = Overrides.ToDictionary(o => o.OccurrenceDate);

        foreach (var date in Recurrence.Dates())
        {
            if (!overridesByDate.TryGetValue(date, out var occurrenceOverride))
            {
                yield return new Occurrence(Id, RoomId, date, StartTime, EndTime, BookedBy);
                continue;
            }

            switch (occurrenceOverride.Kind)
            {
                case OverrideKind.Cancelled:
                case OverrideKind.Skipped:
                    continue;

                case OverrideKind.Moved:
                    // A Moved override without times would silently fall back to the
                    // series time, which is exactly the bug it is meant to record.
                    yield return new Occurrence(
                        Id,
                        RoomId,
                        date,
                        occurrenceOverride.OverrideStartTime ?? throw new InvalidOperationException(
                            $"Moved override {occurrenceOverride.Id} has no start time."),
                        occurrenceOverride.OverrideEndTime ?? throw new InvalidOperationException(
                            $"Moved override {occurrenceOverride.Id} has no end time."),
                        BookedBy);
                    break;
            }
        }
    }
}
