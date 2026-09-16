using ConcertsGraz.Interfaces;
using ConcertsGraz.Models;

namespace ConcertsGraz.Services;

// ==================================================================================
// CLASS: ScraperService - runs all scrapers, collects and returns result 
// ==================================================================================

public class ScraperService
{
    // ATTRIBUTES
    private readonly IEnumerable<IScraper> _scrapers;
    private readonly ILogger<ScraperService> _logger;
    
    // CONSTRUCTOR - di 
    public ScraperService(IEnumerable<IScraper> scrapers, ILogger<ScraperService> logger)
    {
        // scraper instances (via interface to access all in enumerable)
        _scrapers = scrapers;
        _logger = logger;

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
                var scrapedConcerts = await scraper.RunAsync();
                allScrapedConcerts.AddRange(scrapedConcerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(  // use ILogger, integrated ASP.NET Core Logging System
                    ex,
                    "Scraping error in {ScraperName}",
                    scraper.GetType().Name);
            }
            
        }
        
        // return list of all concerts
        return allScrapedConcerts;
    }
    
// END CLASS
}