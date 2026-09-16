
using System.Text.RegularExpressions;
using ConcertsGraz.Interfaces;
using ConcertsGraz.Models;
using HtmlAgilityPack;
using ConcertsGraz.Utilities;
using System.Text.Json;
namespace ConcertsGraz.Scrapers;

// ==================================================================================
// CLASS: HttpScraper: Scrapes Concert Details (PPC pagee)
// ==================================================================================


public class PpcScraper : IScraper
{
    // ATTRIBUTES
    private readonly ILogger<PpcScraper> _logger;
    
    // CONSTRUCTOR - DI logger
    public PpcScraper(ILogger<PpcScraper> logger)
    {
        _logger = logger;
    }
    
    // METHODS
    // run scraper
    public async Task<List<Concert>> RunAsync()
    {
        // 0 Variables
        List<Concert> concertsPpc = new List<Concert>();
        string url = "https://popculture.at/events/kategorie/konzert/"; 
        
        string titleXPath = ".//h3[contains(@class, 'tribe-events-calendar-list__event-title')]//a";
        string linkXPath = ".//h3[contains(@class, 'tribe-events-calendar-list__event-title')]//a";
        string dateXPath = ".//span[contains(@class, 'tribe-event-date-start')]";
        string timeXPath = ".//span[contains(@class, 'tribe-event-time')]";
        string venueXPath = ".//span[contains(@class, 'tribe-events-c-small-cta__price')]";
        string descriptionXPath = ".//div[contains(@class, 'tribe-events-calendar-list__event-description')]";
        string detailsXPath = "//div[contains(@class, 'totalSum')]"; // CONTAINS PRICE INFO!

        // 1 Load HTML from target url
        var web = new HtmlWeb();
        var doc = web.Load(url);
        var eventElementNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'tribe-events-calendar-list__event-row')]"); 
        
        // 2 null check - exception caught in SraperService
        if (eventElementNodes == null)
        {
            throw new InvalidOperationException(
                "Event-Elemente im HTML nicht gefunden. Website hat sich möglicherweise verändert.");
        }
        
        // 3 Filter relevant event (concert) elements via Loop through concerts
        try
        {
            foreach (var concert in eventElementNodes)
            {
                // 3.1 Get raw data 
                string? rawTitle = concert.SelectSingleNode(titleXPath)?.InnerText;
                string rawLink = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? "";
                string rawVenue = "PPC Graz";
                string? rawDate = concert.SelectSingleNode(dateXPath)?.InnerText;
                string? rawTime = concert.SelectSingleNode(timeXPath)?.InnerText;
                string? rawDescription = concert.SelectSingleNode(descriptionXPath)?.InnerHtml;
                string? rawPrice = await GetPriceAsync(rawLink);

                // 3.2 clean variables with ConcertDataSanitizer
                // 3.2.1 all variables except description
                string title = ConcertDataSanitizer.CleanText(rawTitle);
                if (string.IsNullOrWhiteSpace(title)) { continue; } // Skip empty nodes
                if (EventBlacklister.isBlacklisted(title)) { continue; } // skip titles that contain non-concert keywoards
                string venue = ConcertDataSanitizer.CleanVenue(rawVenue);

                // PPC special case: remove "@ 19:00" from date
                string date = ConcertDataSanitizer.CleanText(rawDate);
                date = Regex.Replace(date, @"\s*@.*$", "").Trim();
                DateTime? parsedDate = DateTimeParser.ParseToDateTime(date);

                string time = ConcertDataSanitizer.CleanText(rawTime);
                string price = ConcertDataSanitizer.CleanText(rawPrice);
            
                // PPC special additional logic: remove whitespace in link urls
                string link = Regex.Replace(rawLink, @"\s+", "").Trim();
            
                // 3.2.2 clean description
                // PPC special additional logic: remove JavaScript lefto overs before, remote whitespace in urls
                if (!string.IsNullOrWhiteSpace(rawDescription))
                {
                    rawDescription = Regex.Replace(rawDescription, @"<script\b[^<]*(?:(?!<\/script>)<script\b[^<]*)*<\/script>", "", RegexOptions.IgnoreCase);
                    rawDescription = Regex.Replace(rawDescription, @"window\.\w+[^}]+\}\)", "", RegexOptions.IgnoreCase);
                }
                // clean description with description cleaner
                string description = ConcertDataSanitizer.CleanDescription(rawDescription);

                // 3.3 create new ConcertEvent from scraped element data
                var concertToAdd = new Concert() 
                {
                    Title = title,
                    Genre = "-", 
                    Date = parsedDate,
                    Time = time,
                    Venue = venue,
                    InfoLink = link,
                    Description = description,
                    SourceUrl = url,
                    Price = price, 
                };
            
                // 3.4 add new concert element to venue list
                concertsPpc.Add(concertToAdd);
            
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Konzert von PPC konnte nicht verarbeitet werden und wurde übersprungen.");
        }
        
        // 4 return concerts list
        return concertsPpc;
    }
    
    // FETCH PRICE
    private async Task<string> GetPriceAsync(string detailUrl)
    {
        try
        {
            // load concert detail page
            var detailWeb = new HtmlWeb();
            var detailDoc = detailWeb.Load(detailUrl);

            // extract ticket eventId from script
            var eventIdMatch = Regex.Match(
                detailDoc.DocumentNode.InnerHtml,
                @"eventId:\s*[""']([^""']+)[""']"
            );

            if (!eventIdMatch.Success)
            {
                return "-";
            }

            string eventId = eventIdMatch.Groups[1].Value;

            // build Bringticket API url
            string priceApiUrl =
                $"https://api.checkout.bringticket.com/api/v1/event?eventId={eventId}";

            using var httpClient = new HttpClient();

            // Bringticket requires checkout origin
            httpClient.DefaultRequestHeaders.Add(
                "Origin",
                "https://checkout.bringticket.com"
            );

            // initialize Bringticket and get CSRF token
            using var initResponse = await httpClient.PostAsync(
                "https://api.checkout.bringticket.com/api/v1/payment/init",
                null
            );

            initResponse.EnsureSuccessStatusCode();

            string initJson = await initResponse.Content.ReadAsStringAsync();

            using var initDocument = JsonDocument.Parse(initJson);

            string? csrfToken = initDocument.RootElement
                .GetProperty("token")
                .GetString();

            if (string.IsNullOrWhiteSpace(csrfToken))
            {
                return "-";
            }

            // add CSRF token for event request
            httpClient.DefaultRequestHeaders.Add(
                "X-CSRF-Token",
                csrfToken
            );

            // fetch event data from Bringticket API
            string json = await httpClient.GetStringAsync(priceApiUrl);

            // parse event JSON
            using var jsonDocument = JsonDocument.Parse(json);
            JsonElement root = jsonDocument.RootElement;

            // get price from first category and ticket class
            decimal price = root
                .GetProperty("categories")[0]
                .GetProperty("classes")[0]
                .GetProperty("price")
                .GetDecimal();

            return $"{price:0.00} €";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(  //use asp.net logger!
                ex,
                "PPC: Preis konnte nicht geladen werden - wird auf '-' gesetzt.");

            return "-"; // fallback
        }
    }
    
// END CLASS
}