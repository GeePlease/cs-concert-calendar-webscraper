using ConcertsGraz.Services;
using ConcertsGraz.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConcertsGraz.Controllers;

// ======================================================================
// CLASS: AuthController - API endpoints for the frontend to handle user
// authentication, login, registration, and session management.
// ======================================================================
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    
    // ATTRIBUTES
    private readonly AuthService _authService;
    
    // CONSTRUCTOR
    public AuthController(AuthService authService)
    {
        _authService = authService;
        
    }
    
    // METHODS
    // API POST - Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginData request)
    {
        // check if user already exists (with AuthService)
        var user = await _authService.AuthenticateAsync(request.Username, request.Password);

        // null check, password check
        if (user == null)
        {
            return Unauthorized(new { message = "Benutzername oder Passwort ungültig." });
        }

        // send response to frontend
        //TODO: implement token/ session management
        return Ok(new 
        { 
            token = user.Id, // USER ID NUR PLATZHALTER!!!!!
            username = user.Username 
        });
    }
    
    // HELPERCLASS
    public class LoginData
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
}