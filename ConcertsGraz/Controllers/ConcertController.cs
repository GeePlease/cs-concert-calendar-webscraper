using ConcertsGraz.Models;
using ConcertsGraz.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
namespace ConcertsGraz.Controllers;

// ==================================================================================
// CLASS: ConcertController -API endpoint for the frontend to fetch
// and filter concert listings from MongoDB.
// ==================================================================================

[ApiController]
[Route("api/concerts")]
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
    // GET: api/all concerts
    [HttpGet]
    public async Task<ActionResult<List<Concert>>> GetAll()
    {
        var concerts = await _concertService.GetAllAsync();
        return Ok(concerts);
    }
    
    //GET: api/ single concert
    [HttpGet("{id}")]
    public async Task<ActionResult<Concert>> GetOneById(string id)
    {
        // check if valid MongoDB ObjectId
        if (!ObjectId.TryParse(id, out _)) { return BadRequest("Ungültige Konzert-ID."); }

        var concert = await _concertService.GetOneAsync(id);
        if (concert == null) { return NotFound(); } //null check
        
        return Ok(concert);
    }
    
 
    
    
    
    

}
