using ConcertsGraz.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ConcertsGraz.Data;

namespace ConcertsGraz.Services;

// ==================================================================================
// CLASS: ConcertService - responsible for managing concert data and handling
// database persistence (CRUD operations, UPSERT) via the MongoDB client.
// ==================================================================================

public class ConcertService
{
    // ATTRIBUTES
    // mongo "table" concert variable
    private readonly IMongoCollection<Concert> _concertsCollection;

    // CONSTRUCTOR
    // MongoClient + settings built in constructor
    public ConcertService(IMongoClient mongoClient, IOptions<ConcertsGrazDatabaseSettings> dbSettings)
    {
        // database via mongoclient
        var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        
        // Collection: concerts "table"
        _concertsCollection = database.GetCollection<Concert>(dbSettings.Value.ConcertsCollectionName);
    }

    // METHODS (CRUD FOR DB)
    
    // CREATE: insert new concert
    public async Task CreateAsync(Concert newConcert) =>
        await _concertsCollection.InsertOneAsync(newConcert);

    // READ (all): read all concert data
    public async Task<List<Concert>> GetAllAsync() =>
        await _concertsCollection.Find(_ => true).ToListAsync();

    // READ (single): get single concert
    public async Task<Concert?> GetOneAsync(string id) =>
        await _concertsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    // UPDATE: update (single) concert 
    public async Task UpdateOneAsync(string id, Concert updatedConcert) =>
        await _concertsCollection.ReplaceOneAsync(x => x.Id == id, updatedConcert);

    // DELETE: delete (single) concert
    public async Task RemoveOneAsync(string id) =>
        await _concertsCollection.DeleteOneAsync(x => x.Id == id);
    
    
    
    // SAVE: create new concert for each scraped concert 
    public async Task SaveScrapedConcertsAsync(List<Concert> scrapedConcerts)
    {
        // check if scrape results not null or 0
        if (scrapedConcerts == null || scrapedConcerts.Count == 0)
        {
            Console.WriteLine("[DEBUG] Keine gescrapten Konzerte empfangen.");
            return;
        } 
        
        Console.WriteLine($"[DEBUG] Verarbeite {scrapedConcerts.Count} gescrapte Konzerte...");
        
        // logic for scraped concerts
        foreach (var concert in scrapedConcerts)
        {
            // does concert already exist in db? - no double entries
            var exists = await _concertsCollection.Find(c =>
                c.Title == concert.Title &&
                c.Venue == concert.Venue &&
                c.Date == concert.Date).AnyAsync();

            // skip scraped concert if already exists
            if (exists)
            {
                Console.WriteLine($"[DEBUG] Übersprungen (existiert bereits): {concert.Title} im {concert.Venue}");
                continue;
            } 
            
            // create new for every scraped concert (no doubles)
            Console.WriteLine($"[DEBUG] Füge neues Konzert hinzu: {concert.Title}");
            await CreateAsync(concert);
        }
    }
    
    
    
// END CLASS
}




