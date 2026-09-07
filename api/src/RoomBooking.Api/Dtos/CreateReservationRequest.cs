using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Api.Dtos;

/// <summary>A request to book a room, once or every week for a number of weeks.</summary>
public sealed class CreateReservationRequest : IValidatableObject
{
    [Required]
    [Range(1, int.MaxValue)]
    public int? RoomId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string BookedBy { get; set; } = string.Empty;

    [Required]
    public DateOnly? Date { get; set; }

    [Required]
    public TimeOnly? StartTime { get; set; }

    [Required]
    public TimeOnly? EndTime { get; set; }

    /// <summary>
    /// How many weekly occurrences to book.
    /// </summary>
    [Range(1, 52)]
    public int Weeks { get; set; } = 1;

    /// <summary>
    /// When some weeks are already taken, book the rest anyway instead of refusing the
    /// whole series.
    /// </summary>
    public bool SkipConflicts { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Comparing a nullable is false whenever either side is null, so a field that was
        // left out falls straight through to [Required] instead of being reported twice.
        if (EndTime <= StartTime)
        {
            yield return new ValidationResult(
                "endTime must be later than startTime.", [nameof(EndTime)]);
        }

        if (Date < DateOnly.FromDateTime(DateTime.Now))
        {
            yield return new ValidationResult("date must not be in the past.", [nameof(Date)]);
        }
    }
}
