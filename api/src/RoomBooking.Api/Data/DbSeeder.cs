using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Domain.Entities;

namespace RoomBooking.Api.Data;

/// <summary>
/// Creates the database and puts the rooms in it.
/// </summary>
/// <remarks>
/// EnsureCreated rather than migrations: the schema of this project is disposable and
/// there is no deployed database whose history has to be respected. Anything real wants
/// migrations from the first commit — see the README.
/// </remarks>
public static class DbSeeder
{
    private static readonly Room[] Rooms =
    [
        new() { Name = "Amstel", Capacity = 4 },
        new() { Name = "Grolsch", Capacity = 8 },
        new() { Name = "Heineken", Capacity = 12 },
        new() { Name = "Bavaria", Capacity = 20 },
    ];

    public static async Task SeedAsync(BookingDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);

        // Write-Ahead Logging: writes go to a side file instead of rewriting the database
        // in place, so reads and the single writer get in each other's way far less.
        // SQLite stores the mode in the database header, so it survives restarts and
        // this pragma changes nothing on any run after the first.
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken);

        if (await context.Rooms.AnyAsync(cancellationToken))
        {
            return;
        }

        context.Rooms.AddRange(Rooms);
        await context.SaveChangesAsync(cancellationToken);
    }
}
