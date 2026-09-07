using ConcertsGraz.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
namespace ConcertsGraz.Services;

// ==================================================================================
// CLASS: ConcertService - responsible for managing concert data and handling
// database persistence (CRUD operations) via the MongoDB client.
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
    public async Task<List<Concert>> GetAsync() =>
        await _concertsCollection.Find(_ => true).ToListAsync();

    // READ (single): get single concert
    public async Task<Concert?> GetAsync(string id) =>
        await _concertsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    // UPDATE: update (single) concert 
    public async Task UpdateAsync(string id, Concert updatedConcert) =>
        await _concertsCollection.ReplaceOneAsync(x => x.Id == id, updatedConcert);

    // DELETE: delete (single) concert
    public async Task RemoveAsync(string id) =>
        await _concertsCollection.DeleteOneAsync(x => x.Id == id);
    
    
    
    // SAVE: create new concert for each scraped concert
    public async Task SaveScrapedConcertsAsync(List<Concert> scrapedConcerts)
    {
        foreach (var concert in scrapedConcerts)
        {
            // create for every scraped concert
            await CreateAsync(concert);
        }
    }
    
    
    
// END CLASS
}




