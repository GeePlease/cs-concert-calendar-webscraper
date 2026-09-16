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
    
    // AUTHENTIFICATION + login logic
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
    
    // REGISTRATION logic
    public async Task<(User? User, string? Error)> RegisterAsync(string username, string email, string password)
    {
        username = username?.Trim() ?? string.Empty;
        email = email?.Trim() ?? string.Empty;

        if (!InputValidator.ValidateUsername(username))
        {
            return (null, "Der Benutzername muss 3–50 Zeichen lang sein.");
        }

        if (!InputValidator.ValidateEmail(email))
        {
            return (null, "Bitte gib eine gültige E-Mail-Adresse ein.");
        }

        if (!InputValidator.ValidatePassword(password))
        {
            return (null, "Das Passwort muss 8–50 Zeichen lang sein und mindestens einen Buchstaben und eine Ziffer enthalten.");
        }

        // check if user already exists
        bool userAlreadyExists = await UserAlreadyExists(username);
        if (userAlreadyExists) { return (null, "Dieser Benutzername ist bereits vergeben."); }
        
        //hash password
        string passwordHash = _pwHasher.HashPassword(password);
        
        //if valid create new user
        User newUser = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash
        };
        
        // register new user in db
        await _usersCollection.InsertOneAsync(newUser);
        
        // return new user object
        return (newUser, null);

    }
    
    // HELPER METHOD: check if user already exists in database
    public async Task<bool> UserAlreadyExists(string username)
    {
        var user = await _usersCollection
            .Find(u => u.Username == username)
            .FirstOrDefaultAsync();
        
        return user != null;
    }

    // END CLASS
}
