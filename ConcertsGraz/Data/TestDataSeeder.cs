using ConcertsGraz.Models;
using ConcertsGraz.Utilities;
using MongoDB.Driver;

namespace ConcertsGraz.Data;

public class TestDataSeeder
{
    // ATTRIBUTES
    private readonly IMongoCollection<Concert> _concertCollection;
    private readonly IMongoCollection<User> _userCollection;
    private readonly PasswordHasher _passwordHasher;

    // CONSTRUCTOR: receive dependencies via Dependency Injection
    public TestDataSeeder(IMongoDatabase database, PasswordHasher passwordHasher)
    {
        _concertCollection = database.GetCollection<Concert>("Concerts");
        _userCollection = database.GetCollection<User>("Users");
        _passwordHasher = passwordHasher;
    }

    // -------------------- METHOD: create and insert concerts in db ----------------------
    public async Task seedConcertsAsync()
    {
        // return if db not empty
        if (await _concertCollection.CountDocumentsAsync(_ => true) > 0) return;

        // create test data
        var testConcerts = new List<Concert>
        {
            new Concert
            {
                Title = "Metal Night",
                Genre = "Heavy Metal",
                Date = new DateTime(2026, 10, 15),
                Time = "20:00",
                Venue = "Schlossberg",
                InfoLink = "https://wakuum.graz.at",
                Description = "Lokale Metalbands heizen dem Publikum im Club Wakuum kräftig ein.",
                SourceUrl = "https://wakuum.graz.at",
                Price = "12 €"
            },
            new Concert
            {
                Title = "Indie Night Graz",
                Genre = "Indie",
                Date = new DateTime(2026, 11, 02),
                Time = "19:30",
                Venue = "Café Kork",
                InfoLink = "https://cafe-kork.com/",
                Description = "Ein Abend voll knackiger Gitarrensounds und moderner Synth-Pop-Vibes.",
                SourceUrl = "https://cafe-kork.com/",
                Price = "5 €"
            },
            new Concert
            {
                Title = "Jazz & Wine Session",
                Genre = "Jazz",
                Date = new DateTime(2026, 10, 28),
                Time = "21:00",
                Venue = "Café Stockwerk",
                InfoLink = "https://cafe-stockwerk.at/",
                Description = "Gemütliche Improv-Jazz-Session in intimer Bar-Atmosphäre.",
                SourceUrl = "https://cafe-stockwerk.at/",
                Price = "Eintritt frei"
            }
        };

        await _concertCollection.InsertManyAsync(testConcerts);
    }

    // -------------------- METHOD: create development test user ----------------------
    public async Task seedUserAsync()
    {
        // return if test user already exists
        if (await _userCollection.CountDocumentsAsync(
                u => u.Email == "testuser@example.com") > 0)
        {
            return;
        }

        var testUser = new User
        {
            Username = "testuser",
            Email = "testuser@example.com",
            PasswordHash = _passwordHasher.HashPassword("Test123!")
        };

        await _userCollection.InsertOneAsync(testUser);
    }

    // -------------------- METHOD: seed all development test data ----------------------
    public async Task SeedAllAsync()
    {
        await seedConcertsAsync();
        await seedUserAsync();
    }
}