using ConcertsGraz.Services;
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

        // save id from logged-in user in session
        HttpContext.Session.SetString("UserId", user.Id);

        // send response to frontend
        return Ok(new 
        { 
            token = user.Id, // use to manage login state in frontend
            username = user.Username 
        });
    }
    
    // API POST - Register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterData request)
    {
        var (newUser, error) = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
        if (newUser == null)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { message = "Registrierung erfolgreich!", username = newUser.Username });
    }
    
    
    
    // HELPER CLASS (Data FromBody)
    public class LoginData
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
    // HELPER CLASS
    public class RegisterData
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
}
