using ConcertsGraz.Interfaces;
using ConcertsGraz.Models;
using ConcertsGraz.Scrapers;

namespace ConcertsGraz.Services;

// ==================================================================================
// CLASS: ScraperService - runs all scrapers, collects and returns result 
// ==================================================================================

public class ScraperService
{
    // ATTRIBUTES
    private readonly IEnumerable<IScraper> _scrapers;
    
    // CONSTRUCTOR - di 
    public ScraperService(IEnumerable<IScraper> scrapers)
    {
        // scraper instances (via interface to access all in enumerable)
        _scrapers = scrapers;

    }
    
    // METHODS
    // RUN ALL SCRAPERS and collect in 1 list
    public async Task<List<Concert>> RunAllAsync()
    {
        // List for all concerts (results from scraping)
        var allScrapedConcerts = new List<Concert>();
        
        // run all scrapers via IScrapers Loop
        foreach (var scraper in _scrapers)
        {

            try
            {
                allScrapedConcerts = await scraper.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Scraping-Fehler: {ex.Message}");
            }
            
        }
        
        // return list of all concerts
        return allScrapedConcerts;
    }
    
    
// END CLASS
    
}