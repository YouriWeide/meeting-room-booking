using RoomBooking.Api.Domain.Models;

namespace RoomBooking.Api.Dtos;

public static class OccurrenceMapping
{
    /// <summary>
    /// Maps one occurrence, tagged with how many the whole series still has.
    /// </summary>
    /// <param name="seriesOccurrences">
    /// Must be the size of the reservation's full expansion, not of a filtered view of
    /// it.
    /// </param>
    public static OccurrenceDto ToDto(this Occurrence occurrence, int seriesOccurrences) =>
        new(
            occurrence.ReservationId,
            occurrence.RoomId,
            occurrence.Date,
            occurrence.Start,
            occurrence.End,
            occurrence.BookedBy,
            seriesOccurrences);
}
