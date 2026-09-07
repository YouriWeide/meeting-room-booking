namespace RoomBooking.Api.Dtos;

/// <summary>A bookable room, as the frontend sees it.</summary>
public sealed record RoomDto(int Id, string Name, int Capacity);
