namespace RoomBooking.Api.Domain.Entities;

/// <summary>A bookable meeting room. Seeded, never created through the API.</summary>
public class Room
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public int Capacity { get; set; }
}
