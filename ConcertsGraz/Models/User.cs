using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConcertsGraz.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // pw as hash only

    // save bookmarked concerts and attended concerts via string ids in lists
    public List<string> BookmarkedConcertIds { get; set; } = new();
    public List<string> AttendedConcertIds { get; set; } = new();
}