using ConcertsGraz.Models;
using ConcertsGraz.Services;
using Microsoft.AspNetCore.Mvc;
namespace ConcertsGraz.Controllers;

// ==================================================================================
// CLASS: ConcertController -API endpoint for the frontend to fetch
// and filter concert listings from MongoDB.
// ==================================================================================

[ApiController]
[Route("api/[controller]")]
public class ConcertController : ControllerBase
{
    // ATTRIBUTES
    private readonly ConcertService _concertService;
    
    // CONSTRUCTOR + dependency injection
    public ConcertController(ConcertService concertService)
    {
        _concertService = concertService; 
    }
    
    // METHODS
    // GET: api/concerts
    [HttpGet]
    public async Task<ActionResult<List<Concert>>> GetAll()
    {
        var concerts = await _concertService.GetAllConcertsAsync();
        return Ok(concerts);
    }
    
 
    
    
    
    
    //TODO: CALL CONCERT SERVICE CRUD METHODS WITHIN OTHER LOGICAL METHODS FOR OPERATIONS
}