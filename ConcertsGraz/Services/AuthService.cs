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
    
    // CONSTRUCTOR - DI
    public AuthService(IMongoDatabase database, PasswordHasher passwordHasher)
    {
        // directly load collectection from db
        _usersCollection = database.GetCollection<User>("Users");
        _pwHasher = passwordHasher; 
    }
    
    // METHODS
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        // check if user exists in db (by username)
        var user = await _usersCollection
            .Find(u => u.Username == username)
            .FirstOrDefaultAsync();
        
        // return exsiting user or null
        if (user != null) { return user; }
        return null;
    }

    // END CLASS
}