using HtmlAgilityPack;
using System.Text.RegularExpressions;
using ConcertsGraz.Interfaces; // to remove whitespace
using ConcertsGraz.Models;
using ConcertsGraz.Utilities;
namespace ConcertsGraz.Scrapers;

public class CafeWolfScraper : IScraper
{
    //ATTRIBUTES
    private readonly ILogger<CafeWolfScraper> _logger;
    
    // CONSTRUCTOR
    public CafeWolfScraper(ILogger<CafeWolfScraper> logger)
    {
        _logger = logger;
    }
    
    // METHODS

    public async Task<List<Concert>> RunAsync()
    {
        // 0 Variables
        List<Concert> concertsCafeWolf= new List<Concert>();
        string url = "https://cafewolf.at/programm";
        
        // xpaths relative to main node element
        string titleXPath = ".//h3"; 
        string dateTimeXPath = ".//datetime"; // date + time together...:-/
        string descriptionXPath = ".//p";     
        string linkXPath = "div[2]/div//a";
        
        // 1 Load HTML from target url
        var web = new HtmlWeb();
        var doc = web.Load(url);
        var eventElementNodes = doc.DocumentNode.SelectNodes("//*[@id=\"eventlist\"]//article"); // concert elements to loop through
        
        // 2 Null check
        if (eventElementNodes == null)
        {
            throw new InvalidOperationException(
                "Event-Elemente im HTML nicht gefunden. Website hat sich möglicherweise verändert.");
        }
        
        // 3 Filter relevant event (concert) elements via Loop through concert elements and create concert objects
        foreach (var concert in eventElementNodes)
        {
            try
            {
                // 3.1 get raw data (and trim or "cut")
                string? rawTitle = concert.SelectSingleNode(titleXPath)?.InnerText;
                string rawLink = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? "";
                string? rawDateTime = concert.SelectSingleNode(dateTimeXPath)?.InnerText; //contains both: date and time :/
                string? rawDescription = concert.SelectSingleNode(descriptionXPath)?.InnerHtml; // InnerHtml für HTML-Cleaning
                string rawVenue = "Café Wolf";
                string rawPrice = "-";
            
                // 3.2 Clean variables with ConcertDataSanitizer
                // 3.2.1 Title, Venue & Link
                string title = ConcertDataSanitizer.CleanText(rawTitle);
                title = Regex.Replace(title, @"&AMP;", "&", RegexOptions.IgnoreCase); // Cafe Wolf special: Fix uppercase &AMP;
            
                if (string.IsNullOrWhiteSpace(title)) { continue; } // Skip empty nodes
                if (EventBlacklister.isBlacklisted(title)) { continue; } // skip titles that contain non-concert keywoards
            
                string venue = ConcertDataSanitizer.CleanVenue(rawVenue);
                string link = Regex.Replace(rawLink, @"\s+", "").Trim();
                if (string.IsNullOrWhiteSpace(link)) { link = "-"; }
            
                // 3.2.2 Cafe Wolf special logic case: Extract date and time from single datetime string
                string cleanedDateTime = ConcertDataSanitizer.CleanText(rawDateTime);
                string date = Regex.Match(cleanedDateTime, @"\d{2}\.\d{2}\.\d{4}").Value;
                DateTime? parsedDate = DateTimeParser.ParseToDateTime(date);

                string time = Regex.Match(cleanedDateTime, @"\d{2}:\d{2}").Value;
                string price = ConcertDataSanitizer.CleanText(rawPrice);

                // 3.2.3 Clean description with TextCleaner (description
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
                    Price = price
                };
            
                // 3.4 add new concert element to venue list
                concertsCafeWolf.Add(concertToAdd);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    $"Konzert von Café Wolf konnte nicht verarbeitet werden und wurde übersprungen. " +
                    $"Element betroffen: {concert.OuterHtml}" + " Error Message: " + ex.Message);
            }
            
        }

        // 4 return concert list
        return concertsCafeWolf;
    }
    
// END CLASS   
}