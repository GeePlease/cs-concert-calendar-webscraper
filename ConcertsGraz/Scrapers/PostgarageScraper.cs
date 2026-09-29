using Microsoft.Playwright;
using System.Threading.Tasks;
using ConcertsGraz.Interfaces;
using ConcertsGraz.Models;

namespace ConcertsGraz.Scrapers;

/*Dynamic: USE PLAYWRIGHT!
 
 https://www.zenrows.com/blog/playwright-c-sharp
 https://www.zenrows.com/blog/xpath-web-scraping
 https://playwright.dev/dotnet/docs/api/class-playwright
 https://playwright.dev/dotnet/docs/api/class-page#page-wait-for-load-state
 
 */

public class PostgarageScraper : IScraper
{
    // ATTRIBUTES
    private readonly ILogger<PostgarageScraper> _logger;
    private const string Url = "https://www.postgarage.at/program/";
    
    // CONSTRUCTOR
    public PostgarageScraper(ILogger<PostgarageScraper> logger)
    {
        _logger = logger;
    }
    
    // METHODS
    public async Task<List<Concert>> RunAsync()
    {
        // 0 Variables
        List<Concert> PostgarageConcerts = new();
        
        // paths, elements
        
        // playwright & browser
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        
        // load url
        await page.GotoAsync(Url);
        
        // TODO: rest schreiben
        
        
        
        return PostgarageConcerts;
    }
}