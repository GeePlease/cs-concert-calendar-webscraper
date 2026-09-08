using ConcertsGraz.Models;
using ConcertsGraz.Services;
using ConcertsGraz.Utilities;
using MongoDB.Driver;

namespace ConcertsGraz.Data;

public class TestDataSeeder
{
    // ATTRIBUTES
    private readonly IMongoCollection<User> _userCollection; // entspricht DBSet, Tabelle in SQL
    private readonly IMongoCollection<Concert> _concertCollection; // entspricht DBSet, Tabelle in SQL
    private readonly PasswordHasher _passwordHasher;
    
    // CONSTRUCTOR: pass on DB + PasswordHasher via Dependency Injection
    public TestDataSeeder(IMongoDatabase database, PasswordHasher passwordHasher)
    {
        _userCollection = database.GetCollection<User>("Users");
        _concertCollection = database.GetCollection<Concert>("Concerts");
        _passwordHasher = passwordHasher;
    }
    
    // METHODS
    // -------------------- METHOD: create + insert test Users in DB ----------------------
    public async Task seedUsersAsync()
    {
        // return if collection(s) not empty
        if (await _userCollection.CountDocumentsAsync(_ => true) > 0) return;
        
        // create test data
        var testUsers = new List<User>
        {
            new User
            {
                Username = "max_mustermann",
                Email = "max.mustermann@example.com",
                PasswordHash = _passwordHasher.HashPassword("Password123!")
            },
            new User
            {
                Username = "sara_graz",
                Email = "sara.k@example.at",
                PasswordHash = _passwordHasher.HashPassword("GrazConcerts2026!")
            },
            new User
            {
                Username = "tom_rockt",
                Email = "tom.music@example.com",
                PasswordHash = _passwordHasher.HashPassword("RockOnGraz!")
            }
        };
        
        // insert test data in MongoDB collection
        await _userCollection.InsertManyAsync(testUsers);
    }

    // -------------------- METHOD: create and insert concerts in db----------------------
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
        
        // insert test data in MongoDB collection
        await _concertCollection.InsertManyAsync(testConcerts);
     }
    
    /// -------------------- METHOD: seed users and concerts at once ----------------------
    public async Task SeedAllAsync()
    {
        await seedUsersAsync();  
        await seedConcertsAsync();
    }
    
//END CLASS
}