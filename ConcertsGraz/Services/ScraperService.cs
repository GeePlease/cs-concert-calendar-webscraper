// ==================================================================================
// CLASS: ScraperService - runs all scrapers, collects and returns result 
// ==================================================================================

using ConcertsGraz.Models;
using ConcertsGraz.Scrapers;
namespace ConcertsGraz.Services;

public class ScraperService
{
    // ATTRIBUTES
    // scrapers per site
    private readonly ClubWakuumScraper _clubWakuumScraper;
    private readonly PpcScraper _ppcScraper;
    // private readonly CafeWolfScraper _cafeWolfScraper;
    
    // CONSTRUCTOR - di 
    public ScraperService(ClubWakuumScraper clubWakuScraper, PpcScraper ppcScraper)
    {
        // scraper instances
        _clubWakuumScraper = clubWakuScraper;
        _ppcScraper = ppcScraper;
        //_cafeWolfScraper = careWolfScraper;
    }
    
    // METHODS
    public async Task<List<Concert>> RunAll()
    {
        // List for all concerts (results from scraping)
        var allScrapedConcerts = new List<Concert>();
        
        // Run all Scrapers
        var concertsClubWakuum = await _clubWakuumScraper.RunAsync();
        var concertsPpc  = await _ppcScraper.RunAsync();
        //var concertsWolf = await _cafeWolfScraper.RunAsync();
        
        // combine all list results (by adding single list results to new list)
        allScrapedConcerts.AddRange(concertsClubWakuum);
        allScrapedConcerts.AddRange(concertsPpc);
        
        // return list of all concerts
        return allScrapedConcerts;
    }
    
    
// END CLASS
    
}