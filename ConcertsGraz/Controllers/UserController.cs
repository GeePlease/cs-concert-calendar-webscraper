using ConcertsGraz.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcertsGraz.Controllers;

[Route("api/users")]
public class UserController : ControllerBase
{
    
    // ATTRIBUTES
    private readonly UserService _userService;
    
    // CONSTRUCTOR: di - user service
    public UserController(UserService userService)
    {
        _userService = userService;
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

        // load from db witz id from UserService.cs
        var userToLoad = await _userService.GetSingleUserByIdAsync(userId);

        // handle error
        if (userToLoad == null)
        {
            return BadRequest("Profil konnte nicht geladen werden");
        }

        // handle success,  use "Projection" not real UserObject that includes PasswordHash, Safety!
        return Ok(new
        {
            userToLoad.Id,
            userToLoad.Username,
            userToLoad.Email
        });
    }

    // API PUT (Update single user profile name or email): FromBody = coming from frontend, 
    // using UpdateProfileData Helperclass
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileData updateProfileData)
    {
        // user Id from Session
        var userId = HttpContext.Session.GetString("UserId");
        
        // null check
        if (string.IsNullOrEmpty(userId)) { return Unauthorized("Nicht eingeloogt.");}
        
        
        // update with update method from UserService.cs and HelperClass
        var userToUpdate = await _userService.UpdateNameOrMail(
            userId,
            updateProfileData.Username,
            updateProfileData.Email);
        
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
        );;
        
        // handle error/ success
        if (!success)
        {
            return BadRequest("Fehler bei Aktualisierung des Passworts.");
        }
        
        return Ok("Passwort erfolgreich aktualisiert.");
    }
    
    
    // API DELETE (delete user by id)
    [HttpPut("delete")]
    public async Task<IActionResult> DeleteUserAsync() 
    {
        // user Id from Session
        var userId = HttpContext.Session.GetString("UserId");
        
        // null check
        if (string.IsNullOrEmpty(userId)) { return Unauthorized("Nicht eingeloogt.");}
        
        // delete user from db
        var success = await _userService.DeleteUserAsync(userId);
        
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
    }
    
    //  HELPER CLASS FOR UPDATING USER PASSWORD DATA
    
    public class UpdatePasswordData
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
    
    
    
// END CLASS
}