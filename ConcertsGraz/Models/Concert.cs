// ==================================================================================
// CLASS: CONCERT - concert object 
// --> MongoDB collection
// ==================================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ConcertsGraz.Models;

public class Concert
{
    [BsonId] // Primary Key, Required for mapping the Common Language Runtime (CLR) object to the MongoDB collection.
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } 
    
    public string Title { get; set; } = "";
    public string Genre { get; set; } = "-"; // TODO: extraction method in venue scrapers
    public DateTime? Date { get; set; }
    public string Time { get; set; } = "";
    public string Venue { get; set; } = "";
    public string InfoLink { get; set; } = "";
    public string Description { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string Price { get; set; } = ""; 
}