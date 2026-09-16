using HtmlAgilityPack;
using System.Text.RegularExpressions; // to remove whitespace
using ConcertsGraz.Models;
using ConcertsGraz.Interfaces;
using ConcertsGraz.Utilities;

namespace ConcertsGraz.Scrapers;

// ==================================================================================
// CLASS: HttpScraper: Scrapes Concert Details (club wakuum page)
// ==================================================================================
public class ClubWakuumScraper : IScraper
{
    // ATTRIBUTES
    private readonly ILogger<ClubWakuumScraper> _logger;
    
    // CONSTRUCTOR - DI logger
    public ClubWakuumScraper(ILogger<ClubWakuumScraper> logger)
    {
        _logger = logger;
    }
    
    
    
    // METHODS
    public async Task<List<Concert>> RunAsync()
    {
        // 0 Variables
        List<Concert> concertsClubWakuum = new List<Concert>();
        string url = "https://wakmusic.at/events-konzerte/"; 
        
        // xpaths relative to main node element
        string titleXPath = ".//h3[@class='mec-event-title']/a"; 
        string dateXPath = ".//span[@class='mec-start-date-label']";
        string timeXPath = ".//span[@class='mec-start-time']";
        string venueXPath = ".//div[@class='mec-venue-details']//span";
        string linkXPath = ".//h3[@class='mec-event-title']/a";
        string descriptionXPath = ".//div[contains(@class, 'mec-event-description')]";
        string priceXPath = ".//span[contains(@class, 'mec-label-normal')]";
        
        // 1 Load HTML from target url
        var web = new HtmlWeb();
        var doc = web.Load(url);
        var eventElementNodes = doc.DocumentNode.SelectNodes("//article[contains(@class, 'mec-event-article')]"); // main node element
        
        // 2 null check - exception caught in SraperService
        if (eventElementNodes == null)
        {
            throw new InvalidOperationException(
                "Event-Elemente im HTML nicht gefunden. Website hat sich möglicherweise verändert.");
        }
        
        // 3 Filter relevant event (concert) elements via Loop through concert elements and create concert objects
        try // safer within try catch block
        {
            foreach (var concert in eventElementNodes)
            {
                // 3.1 get raw data (and clean)
                string? rawTitle = concert.SelectSingleNode(titleXPath)?.InnerText;
                string rawLink = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? "";
                string? rawVenue = concert.SelectSingleNode(venueXPath)?.InnerText;
                string? rawDate = concert.SelectSingleNode(dateXPath)?.InnerText;
                string? rawTime = concert.SelectSingleNode(timeXPath)?.InnerText;
                string? rawDescription = concert.SelectSingleNode(descriptionXPath)?.InnerHtml;
                string? rawPrice = concert.SelectSingleNode(priceXPath)?.InnerText;
            
                // 3.2 clean variables with ConcertDataSanitizer + EventBlacklister
                // 3.2.1 all variables except description
                string title = ConcertDataSanitizer.CleanText(rawTitle);
                if (string.IsNullOrWhiteSpace(title)) { continue; } // Skip empty nodes (title is empty)
                if (EventBlacklister.isBlacklisted(title)) { continue; } // skip titles that contain non-concert keywoards
                string venue = ConcertDataSanitizer.CleanVenue(rawVenue);
                string date = ConcertDataSanitizer.CleanText(rawDate);
                DateTime? parsedDate = DateTimeParser.ParseToDateTime(date); // parse date to DateTime Object
                string time = ConcertDataSanitizer.CleanText(rawTime);
                string price = ConcertDataSanitizer.CleanText(rawPrice);
            
                // 3.2.2 description
                string description = ConcertDataSanitizer.CleanDescription(rawDescription);
    
                // 3.2.3 special local step: clean URLs (remove whitespace
                string link = Regex.Replace(rawLink, @"\s+", "").Trim();
         
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
                    Price = price
                };
                
                // 3.4 add new concert element to venue list
                concertsClubWakuum.Add(concertToAdd);

            }
        }
        catch (Exception ex) // catch, log warning, and continue
        {
            _logger.LogWarning(
                ex,
                "Konzert von club wakuum konnte nicht verarbeitet werden und wurde übersprungen.");
            
        }
        // 4 return concert list
        return concertsClubWakuum;
    }
    
// END CLASS

}