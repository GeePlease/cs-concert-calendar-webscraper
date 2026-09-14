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
        if (userToLoad == null)
        {
            return BadRequest("Profil konnte nicht geladen werden");
        }

        return Ok(userToLoad);
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

        if (userToUpdate == null)
        {
            return BadRequest("Fehler bei Aktualisierung des Profils.");}


        return Ok("Profil aktualisiert.");
    }
    
 
    



    // API PUT (update password)
    // API DELETE (delete user by id)
    
    
    //  HELPER CLASS FOR UPDATING USER PROFILE DATA
    
    public class UpdateProfileData
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
    
    
// END CLASS
}