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
        await using var browser = await playwright.Chromium.LaunchAsync(); // adapt to browser! TODO: Firefox!
        var page = await browser.NewPageAsync();
        
        // load url
        await page.GotoAsync(Url);
        
        // TODO: rest schreiben
        // 1 choose /"click" month from month selector navigation <li class="active">
        //<a href="/program/?from=2026-10-01&amp;till=2026-11-01" style="background-position-y: -50.2951px;"><span class="short">Oct</span><span class="long">October</span></a></li>
        // active is start index for loop over #month-selector element
        // 3 find month click urls and save in month url list
        // 4 loop through month nav url list and collect concert event detail links into list
        // 5 loop thorough conct event detail link list
        // 5.1 load
        // 5.2 standard exatraction procedure (compare to Wakuum e.g.)
        
        return PostgarageConcerts;
    }
}