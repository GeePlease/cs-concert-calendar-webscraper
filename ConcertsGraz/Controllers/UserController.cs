using ConcertsGraz.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
namespace ConcertsGraz.Controllers;

// ==================================================================================
// CLASS: UserController - API endpoints for managing user profile details,
// updating account credentials, and handling user account deletion.
// ==================================================================================

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    
    // ATTRIBUTES
    private readonly UserService _userService;
    private readonly ConcertService _concertService;
    
    // CONSTRUCTOR: di - user service
    public UserController(UserService userService, ConcertService concertService)
    {
        _userService = userService;
        _concertService = concertService;
    }
    
    // API GET (Single user by name or email) api/users/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        // user Id from Session
        var userId = HttpContext.Session.GetString("UserId");

        // null check
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Nicht eingeloggt.");
        }

        // load from db with id from UserService.cs
        var userToLoad = await _userService.GetSingleUserByIdAsync(userId);

        // handle error
        if (userToLoad == null)
        {
            return NotFound("Profil konnte nicht geladen werden");
        }

        // handle success,  use "Projection" not real UserObject that includes PasswordHash, Safety!
        return Ok(new
        {
            userToLoad.Id,
            userToLoad.Username,
            userToLoad.Email
        });
    }
    
    // API GET (get all bookmarked concert ids)
    [HttpGet("bookmarks")]
    public async Task<IActionResult> GetBookmarkedConcertsAsync()
    {
        // get user id from session
        var userId = HttpContext.Session.GetString("UserId");

        // null check
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Nicht eingeloggt.");
        }

        // get user from db
        var userToLoad = await _userService.GetSingleUserByIdAsync(userId);

        // handle error
        if (userToLoad == null)
        {
            return NotFound("User konnte nicht gefunden werden.");
        }

        // return bookmarked concert ids
        return Ok(userToLoad.BookmarkedConcertIds);
    }
    
    // API POST (bookmark concert)
    [HttpPost("bookmarks/{concertId}")]
    public async Task<IActionResult> BookmarkConcertAsync(string concertId)
    {
        // get user id from session
        var userId = HttpContext.Session.GetString("UserId");

        // null check
        if (string.IsNullOrEmpty(userId)) { return Unauthorized("Nicht eingeloggt."); }
        
        // check if valid MongoDB ObjectId
        if (!ObjectId.TryParse(concertId, out _)) { return BadRequest("Ungültige Konzert-ID."); }

        var concert = await _concertService.GetOneAsync(concertId);
        if (concert == null) { return NotFound("Konzert konnte nicht gefunden werden."); }

        // bookmark concert
        var success = await _userService.BookmarkConcertId(userId, concertId);

        // handle error
        if (!success)
        {
            return BadRequest("Konzert konnte nicht vorgemerkt werden.");
        }

        // handle success
        return Ok("Konzert erfolgreich vorgemerkt.");
    }
    
    // API DELETE (unbookmark concert)
    [HttpDelete("bookmarks/{concertId}")]
    public async Task<IActionResult> UnbookmarkConcertAsync(string concertId)
    {
        // get user id from session
        var userId = HttpContext.Session.GetString("UserId");

        // null check
        if (string.IsNullOrEmpty(userId)) { return Unauthorized("Nicht eingeloggt."); }

        // unbookmark concert
        var success = await _userService.UnbookmarkConcertId(userId, concertId);

        // handle error
        if (!success)
        {
            return BadRequest("Konzert konnte nicht entfernt werden.");
        }

        // handle success
        return Ok("Konzert erfolgreich entfernt.");
    }
    

    // API PUT (Update single user profile name or email): FromBody = coming from frontend, 
    // using UpdateProfileData Helperclass
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileData updateProfileData)
    {
        // user Id from Session
        var userId = HttpContext.Session.GetString("UserId");
        
        // null check
        if (string.IsNullOrEmpty(userId)) { return Unauthorized("Nicht eingeloggt.");}
        
        
        // update with update method from UserService.cs and HelperClass
        var userToUpdate = await _userService.UpdateNameOrMail(
            userId,
            updateProfileData.Username,
            updateProfileData.Email,
            updateProfileData.CurrentPassword);
        
        // handle error/success
        if (userToUpdate == null)
        {
            return BadRequest("Fehler bei Aktualisierung des Profils.");}

        return Ok("Profil aktualisiert.");
    }

    // API PUT (update password)
    [HttpPut("updatePW")]
    public async Task<IActionResult> UpdatePasswordAsync([FromBody] UpdatePasswordData updatePasswordData)
    {
        // get user id from session
        var userId = HttpContext.Session.GetString("UserId");
        
        // null check
        if (string.IsNullOrEmpty(userId)) { return Unauthorized("Nicht eingeloggt."); }
        
        // update user password
        var success = await _userService.UpdateUserPassword(
            userId, 
            updatePasswordData.CurrentPassword, 
            updatePasswordData.NewPassword
        );
        
        // handle error/ success
        if (!success)
        {
            return BadRequest("Fehler bei Aktualisierung des Passworts.");
        }
        
        return Ok("Passwort erfolgreich aktualisiert.");
    }
    
    
    // API DELETE (delete user by id)
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteUserAsync([FromBody] DeleteUserData? deleteUserData) //BOdy = response body
    {
        // user Id from Session
        var userId = HttpContext.Session.GetString("UserId");
        
        // null check
        if (string.IsNullOrEmpty(userId)) { return Unauthorized("Nicht eingeloggt.");}
        
        // delete user from db
        var success = await _userService.DeleteUserAsync(userId, deleteUserData?.CurrentPassword);
        
        // handle error
        if (!success)
        {
            return BadRequest($"User mit ID '{userId}' konnte nicht gelöscht werden.");
        }
        
        // clear session (because user deleted and not logged in anymore)
        HttpContext.Session.Clear();
        
        // handle success
        return Ok("User erfolgreich gelöscht");
    }
    
    
    
    
    //  HELPER CLASS FOR UPDATING USER PROFILE DATA
    
    public class UpdateProfileData
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? CurrentPassword { get; set; }
    }
    
    //  HELPER CLASS FOR UPDATING USER PASSWORD DATA
    
    public class UpdatePasswordData
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    // HELPER CLASS FOR DELETING USER ACCOUNT
    public class DeleteUserData
    {
        public string? CurrentPassword { get; set; }
    }
    
    
    
// END CLASS
}
