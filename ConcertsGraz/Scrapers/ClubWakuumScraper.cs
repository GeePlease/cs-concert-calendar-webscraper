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

    public async Task<List<Concert>> RunAsync()
    {
        // 0 Variables
        List<Concert> concertsClubWakuum = new List<Concert>();
        string url = "https://wakmusic.at/events-konzerte/"; 
        
        // xpaths relative to main node element
        string titleXPath = ".//h3[@class='mec-event-title']/a"; 
        string dateXPath = ".//span[@class='mec-start-date-label']";
        string timeXPath = ".//span[@class='mec-start-time']";
        string venueXPath = ".//div[@class='mec-venue-details']";
        string linkXPath = ".//h3[@class='mec-event-title']/a";
        string descriptionXPath = ".//div[contains(@class, 'mec-event-description')]";
        string priceXPath = ".//span[contains(@class, 'mec-label-normal')]";
        
        // 1 Load HTML from target url
        var web = new HtmlWeb();
        var doc = web.Load(url);
        var eventElementNodes = doc.DocumentNode.SelectNodes("//article[contains(@class, 'mec-event-article')]"); // main node element
        
        // 2 Filter relevant event (concert) elements via Loop through concert elements and create concert objects
        foreach (var concert in eventElementNodes)
        {
            // 2.1 get raw data (and clean)
            string? rawTitle = concert.SelectSingleNode(titleXPath)?.InnerText;
            string rawLink = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? "";
            string? rawVenue = concert.SelectSingleNode(venueXPath)?.InnerText;
            string? rawDate = concert.SelectSingleNode(dateXPath)?.InnerText;
            string? rawTime = concert.SelectSingleNode(timeXPath)?.InnerText;
            string? rawDescription = concert.SelectSingleNode(descriptionXPath)?.InnerHtml;
            string? rawPrice = concert.SelectSingleNode(priceXPath)?.InnerText;
            
            // 2.2 clean variables with TextCleaner
            // 2.2.1 all variables except description
            string title = TextCleaner.CleanText(rawTitle).ToUpper();
            if (string.IsNullOrWhiteSpace(title)) { continue; } // Skip empty nodes
            string venue = TextCleaner.CleanText(rawVenue);
            string date = TextCleaner.CleanText(rawDate);
            DateTime? parsedDate = DateTimeParser.ParseToDateTime(date); // parse date to DateTime Object
            string time = TextCleaner.CleanText(rawTime);
            string price = TextCleaner.CleanText(rawPrice);
            
            // 2.2.2 description
            string description = TextCleaner.CleanDescription(rawDescription);
    
            // 2.2.3 special local step: clean URLs (remove whitespace
            string link = Regex.Replace(rawLink, @"\s+", "").Trim();
         
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
                Price = price
            };
                
            // 2.4 add new concert element to venue list
            concertsClubWakuum.Add(concertToAdd);
            
            // print elements in console TODO: REMOVE LATER
            Console.WriteLine($"Title: {title}, Venue: {venue}, Date: {parsedDate}, Time: {time}, Price: {price}, Description: {description}, Link: {link}\n");

        }
        // 3 return concert list
        return concertsClubWakuum;
    }
    
// END CLASS

}