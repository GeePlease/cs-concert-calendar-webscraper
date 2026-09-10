using ConcertsGraz.Models;
using ConcertsGraz.Utilities;
using MongoDB.Driver;

namespace ConcertsGraz.Services;

// ==================================================================================
// CLASS: NAME + SHORT DESCRIPTION
// ==================================================================================

public class AuthService
{
    // ATTRIBUTES
    private readonly IMongoCollection<User> _usersCollection;
    private readonly PasswordHasher _pwHasher;
    
    // CONSTRUCTOR
    public AuthService(IMongoDatabase database, PasswordHasher passwordHasher)
    {
        // 1. Einmalig beim Erstellen des Services die "Users"-Tabelle greifen
        _users = database.GetCollection<User>("Users");
        _pwHasher = passwordHasher; 
    }
    
    // METHODS
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        
    }

}