using System.Text.RegularExpressions;
using Microsoft.Playwright;
using System.Threading.Tasks;
using ConcertsGraz.Interfaces;
using ConcertsGraz.Models;
using ConcertsGraz.Utilities;

namespace ConcertsGraz.Scrapers;

/*Dynamic: USE PLAYWRIGHT!
 
 https://www.zenrows.com/blog/playwright-c-sharp
 https://www.zenrows.com/blog/xpath-web-scraping
 https://playwright.dev/dotnet/docs/api/class-playwright
 https://playwright.dev/dotnet/docs/api/class-page#page-wait-for-load-state
 
 */

public class PostgarageScraper : IScraper
{
    //ATTRIBUTES
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
        
        // playwright & browser
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(); // adapt to browser! TODO: Firefox!
        var page = await browser.NewPageAsync();
        
        // selectors for concert data variable elements 
        string titleElement = "h2.event-name";
        string dateElement = "time.event-date";
        string timeElement = "div.event-time";
        string descriptionElement = "section.info p";
        string genreElement = "p.event-genre";
        string priceElement = "div.admission p";
        string infoLinkElement = "section.links a";
        string venue = "Postgarage";
        
        // navigate to url
        var response1 = await page.GotoAsync(Url);
        
        // TODO: rest schreiben
        // 1.0 select month navigation and find current month: from:  .#month-selector 
        var monthSelector = page.Locator("#month-selector");
        var monthItems = monthSelector.Locator("ul > li");
        var activeMonth = monthSelector.Locator("ul > li.active");
        
        // 1.1 active is start index for loop over #month-selector element: .#month-selector > ul > li.activ
        int monthCount = await monthItems.CountAsync(); // count month items
        int activeIndex = -1; // initialize index as invalid index!

        // loop through months, find active month, save index
        for (int i = 0; i < monthCount; i++)
        {
            // get current month element by index
            var month = monthItems.Nth(i);

            // get class attribute of current month element
            string? classAttribute = await month.GetAttributeAsync("class");

            // check if current month element is current month (marked as "active" class"
            if (classAttribute?.Contains("active") == true)
            {
                activeIndex = i;
                break;
            }
        }
        
        // 2 find month urls and save in month url list: .#month-selector > ul > li:nth-child(5) > a
        List<string> monthUrls = new List<string>(); // list for month urls
        
        // loop through event element list per month
        for (int i = activeIndex; i < monthCount; i++)
        {
            // get current month element by index
            var month = monthItems.Nth(i);

            // find link element inside current month
            var monthLink = month.Locator("a");

            // get href attribute from link
            string? monthUrl = await monthLink.GetAttributeAsync("href");

            // add url to month url list if available
            if (!string.IsNullOrWhiteSpace(monthUrl))
            {
                monthUrls.Add(monthUrl.Trim());
            }
        }
        
        // 3 loop through month nav url list and collect concert event detail links into list:
        // #maincontent > section:nth-child(1) > div:nth-child(1) > time > a
        
        List<string> concertDetailUrls = new List<string>(); // list for concert detail urls
        
        // lo through monthly event views
        foreach (string monthUrl in monthUrls)
        {
            // navigate to current month url
            var response2 = await page.GotoAsync(monthUrl);

            // find concert event link elements on current month page
            var concertLinks = page.Locator("#maincontent > section > div > time > a");

            // count concert event link elements
            int concertCount = await concertLinks.CountAsync();

            // loop through concert event links
            for (int i = 0; i < concertCount; i++)
            {
                // get current concert link element by index
                var concertLink = concertLinks.Nth(i);

                // get href attribute from concert link
                string? concertUrl = await concertLink.GetAttributeAsync("href");

                // add url to concert url list if available
                if (!string.IsNullOrWhiteSpace(concertUrl))
                {
                    concertDetailUrls.Add(concertUrl);
                }
            }
        }
        
        // 4 loop through contact event detail link list (concertDetailUrls) to get concert details
        foreach (var detailUrl in concertDetailUrls)
        {
            // 4.1 load concert event detail link
            var response3 = await page.GotoAsync(detailUrl);
            
            // 4.2 standard extraction procedure (compare to Wakuum e.g.) 
            
            // 4.2.0 null check, skip broken or invalid detail links
            if (response3 != null) {continue;} 
            
            // 4.2.1 get raw data as strings 
            string? rawTitle = await page.Locator(titleElement).TextContentAsync();
            string? rawDate = await page.Locator(dateElement).TextContentAsync();
            string? rawTime = await page.Locator(timeElement).TextContentAsync();
            string? rawGenre = await page.Locator(genreElement).TextContentAsync();
            string? rawDescription = await page.Locator(descriptionElement).TextContentAsync();
            string? rawPrice = await page.Locator(priceElement).TextContentAsync();
            string? rawInfoLink = await page.Locator(infoLinkElement).TextContentAsync();
            
            // 4.2.2 clean variables with ConcertDataSanitizer + EventBlacklister
            // all variables except description
            string title = ConcertDataSanitizer.CleanText(rawTitle);
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            } // Skip empty nodes (title is empty)

            if (EventBlacklister.isBlacklisted(title))
            {
                continue; // skip titles that contain non-concert keywoards
            } 
            
            string date = ConcertDataSanitizer.CleanText(rawDate);
            DateTime? parsedDate = DateTimeParser.ParseToDateTime(date); // parse date to DateTime Object
            string time = ConcertDataSanitizer.CleanText(rawTime);
            string price = ConcertDataSanitizer.CleanText(rawPrice);
            string link = Regex.Replace(rawInfoLink, @"\s+", "").Trim();

            // description
            string description = ConcertDataSanitizer.CleanDescription(rawDescription);

            // add special local step here if necessary
            
            // 4.3 create new ConcertEvent from scraped element data 
            var concertToAdd = new Concert()
            {
                Title = title,
                Genre = "-",
                Date = parsedDate,
                Time = time,
                Venue = venue,
                InfoLink = link,
                Description = description,
                SourceUrl = Url,
                Price = price
            };
            
            // 4.4 add new concert to postgarage concertlist
            PostgarageConcerts.Add(concertToAdd);

        }
        // return concert list
        return PostgarageConcerts;
    }
    
// END CLASS
}