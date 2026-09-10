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
public class AuthController
{
    
    // ATTRIBUTES
    private readonly AuthService _authService;
    
    // CONSTRUCTOR
    public AuthController(AuthService authService)
    {
        _authService = authService;
        
    }
    
    // METHODS
    
    
    
}