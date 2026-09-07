using ConcertsGraz.Models;
using ConcertsGraz.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcertsGraz.Controllers;

// ==================================================================================
// CLASS: ConcertController - routing + calling CRUD methods from ConcertService
// ==================================================================================
public class ConcertController : Controller
{
    // ATTRIBUTES
    private readonly ConcertService _concertService;
    
    // CONSTRUCTOR + dependency injection
    public ConcertController(ConcertService concertService)
    {
        _concertService = concertService; 
    }
    
    // METHODS
    
 
    
    
    
    
    //TODO: CALL CONCERT SERVICE CRUD METHODS WITHIN OTHER LOGICAL METHODS FOR OPERATIONS
}