using System.Text.RegularExpressions;
using ConcertsGraz.Interfaces;
using ConcertsGraz.Models;
using HtmlAgilityPack;
using ConcertsGraz.Utilities;

namespace ConcertsGraz.Scrapers;

public class PpcScraper : IScraper
{
    
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
        
        // 2 Filter relevant event (concert) elements via Loop through concerts
        foreach (var concert in eventElementNodes)
        {
            // 2.1.1 Get raw data (except price - on other site)
            string? rawTitle = concert.SelectSingleNode(titleXPath)?.InnerText;
            string rawLink = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? "";
            string rawVenue = "PPC Graz";
            string? rawDate = concert.SelectSingleNode(dateXPath)?.InnerText;
            string? rawTime = concert.SelectSingleNode(timeXPath)?.InnerText;
            string? rawDescription = concert.SelectSingleNode(descriptionXPath)?.InnerHtml;
            string? rawPrice = "-";
            
            // 2.1.2 load details page to fetch price 
            if (!string.IsNullOrEmpty(rawLink))
            {
                var detailWeb = new HtmlWeb();
                var detailDoc = detailWeb.Load(rawLink);
                var priceNode = detailDoc.DocumentNode.SelectSingleNode(detailsXPath);
                if (priceNode != null)
                {
                    rawPrice = priceNode.InnerText;
                }
            }

            // 2.2 clean variables with TextCleaner
            // 2.2.1 all variables except description
            string title = ConcertDataSanitizer.CleanText(rawTitle);
            if (string.IsNullOrWhiteSpace(title)) { continue; } // Skip empty nodes
            if (EventBlacklister.isBlacklisted(title)) { continue; } // skip titles that contain non-concert keywoards
            string venue = ConcertDataSanitizer.CleanText(rawVenue);

            // PPC special case: remove "@ 19:00" from date
            string date = ConcertDataSanitizer.CleanText(rawDate);
            date = Regex.Replace(date, @"\s*@.*$", "").Trim();
            DateTime? parsedDate = DateTimeParser.ParseToDateTime(date);

            string time = ConcertDataSanitizer.CleanText(rawTime);
            string price = ConcertDataSanitizer.CleanText(rawPrice);
            
            // PPC special additional logic: remove whitespace in link urls
            string link = Regex.Replace(rawLink, @"\s+", "").Trim();
            
            // 2.2.2 clean description
            // PPC special additional logic: remove JavaScript lefto overs before, remote whitespace in urls
            if (!string.IsNullOrWhiteSpace(rawDescription))
            {
                rawDescription = Regex.Replace(rawDescription, @"<script\b[^<]*(?:(?!<\/script>)<script\b[^<]*)*<\/script>", "", RegexOptions.IgnoreCase);
                rawDescription = Regex.Replace(rawDescription, @"window\.\w+[^}]+\}\)", "", RegexOptions.IgnoreCase);
            }
            // clean description with description cleaner
            string description = ConcertDataSanitizer.CleanDescription(rawDescription);

            // 2.3 create new ConcertEvent from scraped element data
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
            
            // 2.4 add new concert element to venue list
            concertsPpc.Add(concertToAdd);
            
            // print elements in console TODO: REMOVE LATER
            Console.WriteLine($"Title: {title}, Venue: {venue}, Date: {parsedDate}, Time: {time}, Description: {description}, Price: {price}, Link: {link}\n");
        }
        
        // 3 return concerts list
        return concertsPpc;
    }
    
    
}