using ConcertsGraz.Services;
using Microsoft.AspNetCore.Mvc;
namespace ConcertsGraz.Controllers;

public class ScraperController : Controller
{
    // ATTRIBUTES
    private readonly ScraperService _scraperService;
    private readonly ConcertService _concertService;
    
    // CONSTRUCTOR - DI
    public ScraperController(ScraperService scraperService, ConcertService concertService)
    {
        _scraperService =  scraperService;
        _concertService = concertService;
    }
    
    // METHODS
    
    // Scrape and Save result in DB
    [HttpPost("run")]
    public async Task<IActionResult> ScrapeAndStoreAsync()
    {
        // scrape and get all individual concert result lists from scraper service
        var scrapedConcerts = await _scraperService.RunAll();
        // create combined results list, from concert service
        await _concertService.SaveScrapedConcertsAsync(scrapedConcerts);
        // feedback
        return Ok($"Erfolg. {scrapedConcerts.Count} Konzerte gescraped und gespeichert.");
    }
    
// END CLASS
}