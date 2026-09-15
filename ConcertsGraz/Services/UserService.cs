using ConcertsGraz.Data;
using ConcertsGraz.Models;
using ConcertsGraz.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    public UserService(IMongoClient mongoClient, IOptions<ConcertsGrazDatabaseSettings> dbSettings, PasswordHasher passwordHasher)
    {
        // database + settings via mongoclient
        var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        
        // Collection: users "table" via db settings !
        _usersCollection = database.GetCollection<User>(dbSettings.Value.UsersCollectionName);
        _pwHasher = passwordHasher;
    }

    
    // METHODS
    
    // GET SINGLE USER FROM DB BY USERNAME
    public async Task<User> GetSingleUserByIdAsync(string userId)
    {
        if (userId == null) { return null; } // null check
        var wantedUser =  await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return wantedUser;
    }
    
    
    // UPDATE USER PROFILE - Username or Email (userId, newUsername, newEmail)
    public async Task<User> UpdateNameOrMail(string userId, string? newUsername, string? newEmail, string? currentPassword)
    {
        // TODO: email validator!
        
        // get user from db
        var wantedUser = await GetSingleUserByIdAsync(userId);
        
        // user null check after search
        if (wantedUser == null) { return null; } 

        if (!string.IsNullOrEmpty(newEmail) &&
            (string.IsNullOrWhiteSpace(currentPassword) ||
             !_pwHasher.VerifyPassword(wantedUser.PasswordHash, currentPassword)))
        {
            return null;
        }
        
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
    
    // DELETE USER 
    public async Task<bool> DeleteUserAsync(string userId)
    {
        // search by id (no extra db anfrage!! less db anfragen = better)
        if (string.IsNullOrWhiteSpace(userId)) { return false; }
        
        // access db and delete user object
        var success = await _usersCollection.DeleteOneAsync(u => u.Id == userId);
        
        // mange error/ success
        if (success.DeletedCount == 0) { return false; } // delete unsuccessfull
        return true; // delete sucessfull

    }
  
    
    // UPDATE USER - Password (string input)
    public async Task<bool> UpdateUserPassword(string userId, string? currentPassword, string? newPassword)
    {
        // basic input check
        if (string.IsNullOrWhiteSpace(userId) || 
            string.IsNullOrWhiteSpace(currentPassword) || 
            string.IsNullOrWhiteSpace(newPassword))
        { return false; }
        
        // TODO: validator
        //  get user from db
        var wantedUser = await GetSingleUserByIdAsync(userId);
       if (wantedUser == null) { return false; }
       
       
        // verify current password
        var result = _pwHasher.VerifyPassword(wantedUser.PasswordHash, currentPassword);
        if(!result) { return false; }
        
        // check if new and old are the same
        if (currentPassword == newPassword) { return false;}
        
        // hash (and validate) new password
        string newPasswordHash = _pwHasher.HashPassword(newPassword);
        
        // update user object
        wantedUser.PasswordHash = newPasswordHash;
        
        // update user object in mongo db
        await _usersCollection.ReplaceOneAsync(u => u.Id == wantedUser.Id, wantedUser);
        
        // manage error/success
        return true;
    }
    

    // BOOKMARK Concerts (get concert id from Api UserController and add to bookmarked concert list in User DB Object)
    public async Task<bool> BookmarkConcertId(string userId, string concertToBookmark) 
    {
        // null check
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(concertToBookmark))
        {
            return false;
        }
    
        // get user from db
        var wantedUser = await GetSingleUserByIdAsync(userId);
        if (wantedUser == null) { return false; } // user not found null check
    
        // check if concert is already bookmarked
        if (wantedUser.BookmarkedConcertIds.Contains(concertToBookmark))
        {
            return true;
        }
    
        // add concertId to list of bookmarked concerts in user object
        wantedUser.BookmarkedConcertIds.Add(concertToBookmark);
    
        // update user data in db
        var result = await _usersCollection.ReplaceOneAsync(u => u.Id == wantedUser.Id, wantedUser);
    
        // manage error/ success
        // user (bookmarked concerts) update not successful
        if (!result.IsAcknowledged || result.ModifiedCount == 0)
        {
            return false;
        }
        // successful
        return true;
    }

//END CLASS
}
