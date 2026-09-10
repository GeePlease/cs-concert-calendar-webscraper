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
    // authentification
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        // check if user exists in db (by username)
        var user = await _usersCollection
            .Find(u => u.Username == username)
            .FirstOrDefaultAsync();
        
        // null check
        if (user == null) { return null; }
        
        //pw verification (pw == pw hash in db?)
        bool passwordIsValid = _pwHasher.VerifyPassword(user.PasswordHash, password);
        if (!passwordIsValid) { return null; } //wrong pw

        return user; //correct pw
        
    }

    // END CLASS
}