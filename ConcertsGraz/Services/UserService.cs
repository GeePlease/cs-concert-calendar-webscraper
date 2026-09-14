using ConcertsGraz.Data;
using ConcertsGraz.Models;
using ConcertsGraz.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
namespace ConcertsGraz.Services;

// ==================================================================================
// CLASS: UserService: all things User related (load user data from db, update
// user profile (name, email, password incl. hashing)
// ==================================================================================

public class UserService
{
    // ATTRIBUTES
    // mongo "table" users variable, pwhasher instance
    private readonly IMongoCollection<User> _usersCollection;
    private readonly PasswordHasher _pwHasher;
    
    // CONSTRUCTOR
    // DI - db, pw hasher
    public UserService(IMongoClient mongoClient, IOptions<ConcertsGrazDatabaseSettings> dbSettings)
    {
        // database + settings via mongoclient
        var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        
        // Collection: users "table"
        _usersCollection = database.GetCollection<User>(dbSettings.Value.ConcertsCollectionName);
    }

    
    // METHODS
    
    // GET SINGLE USER FROM DB BY USERNAME
    public async Task<User> GetSingleUserByIdAsync(string userId)
    {
        if (userId == null) { return null; } // null check
        var wantedUser =  await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return wantedUser;
    }
    
    
    // UPDATE USER PROFILE - Username (userId, newUsername, newEmail)
    public async Task<User> UpdateNameOrMail(string userId, string? newUsername, string? newEmail)
    {
        // TODO: email validator!
        // null check before search
        if (userId == null) { return null; } 
        
        // get user from db
        var wantedUser = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        
        // user null check after search
        if (wantedUser == null) { return null; } 
        
        // TODO: DUPLICATE CHECK!! (decide how to handle double mail + double username)
        // TODO: IMPLEMENT VALIDATOR!!
        
        // update user properties in object (username & email at once possible, not mandatory)
        if (!string.IsNullOrEmpty(newUsername))
        {
            wantedUser.Username = newUsername;
        }
    
        if (!string.IsNullOrEmpty(newEmail))
        {
            wantedUser.Email = newEmail;
        }
        
        // update object in mongo db
        await _usersCollection.ReplaceOneAsync(u => u.Id == wantedUser.Id, wantedUser);
        
        // return updated user object
        return wantedUser; 
        
    }


    
    // UPDATE USER - Password (string input)
    // search user by...id? (best?) search or nullcheck 
    // update found user.password with input string --> USER HASHER! // TODO: validator
    // manage error/success
    
    // DELETE USER 
    // search by id? (best? ) search? nullcheck necessary when loggind in
    // access db and delete user object
    // mange error/ success
    
    
//END CLASS
}