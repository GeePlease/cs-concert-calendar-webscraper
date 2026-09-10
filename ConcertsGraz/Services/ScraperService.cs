using ConcertsGraz.Models;
using ConcertsGraz.Scrapers;

namespace ConcertsGraz.Services;

// ==================================================================================
// CLASS: ScraperService - runs all scrapers, collects and returns result 
// ==================================================================================

public class ScraperService
{
    // ATTRIBUTES
    // scrapers per site
    private readonly ClubWakuumScraper _clubWakuumScraper;
    private readonly PpcScraper _ppcScraper;
    private readonly CafeWolfScraper _cafeWolfScraper;
    // private readonly CafeWolfScraper _cafeWolfScraper;
    
    // CONSTRUCTOR - di 
    public ScraperService(ClubWakuumScraper clubWakuumScraper, PpcScraper ppcScraper, CafeWolfScraper cafeWolfScraper)
    {
        // scraper instances
        _clubWakuumScraper = clubWakuumScraper;
        _ppcScraper = ppcScraper;
        _cafeWolfScraper = cafeWolfScraper;
        
    }
    
    // METHODS
    public async Task<List<Concert>> RunAll()
    {
        // List for all concerts (results from scraping)
        var allScrapedConcerts = new List<Concert>();
        
        // Run all Scrapers
        var concertsClubWakuum = await _clubWakuumScraper.RunAsync();
        var concertsPpc  = await _ppcScraper.RunAsync();
        var concertsWolf = await _cafeWolfScraper.RunAsync();
        
        // combine all list results (by adding single list results to new list)
        allScrapedConcerts.AddRange(concertsClubWakuum);
        allScrapedConcerts.AddRange(concertsPpc);
        allScrapedConcerts.AddRange(concertsWolf);
        
        // return list of all concerts
        return allScrapedConcerts;
    }
    
    
// END CLASS
    
}