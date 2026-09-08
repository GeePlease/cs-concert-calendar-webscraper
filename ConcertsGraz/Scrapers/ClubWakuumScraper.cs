using HtmlAgilityPack;
using System.Text.RegularExpressions; // to remove whitespace
using ConcertsGraz.Models;
using ConcertsGraz.Interfaces;

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
        var eventElementNodes = doc.DocumentNode.SelectNodes("//article[contains(@class, 'mec-event-article')]");
        
        // 2 Filter relevant event (concert) elements via Loop through concert elements and create concert objects
        foreach (var concert in eventElementNodes)
        {
            // 2.1 get raw data (and trim)
            string title = concert.SelectSingleNode(titleXPath)?.InnerText.Trim().ToUpper() ?? "";
            if (string.IsNullOrWhiteSpace(title)) {continue;} // skip empty nodes
            string link = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? "";
            string venue = concert.SelectSingleNode(venueXPath)?.InnerText.Trim() ?? "";
            string date = concert.SelectSingleNode(dateXPath)?.InnerText.Trim() ?? "";
            string time = concert.SelectSingleNode(timeXPath)?.InnerText.Trim() ?? "";
            string description = concert.SelectSingleNode(descriptionXPath)?.InnerHtml.Trim() ?? "";
            string price = concert.SelectSingleNode(priceXPath)?.InnerText.Trim() ?? "";
            
            // 2.2 clean variables (No Whitespace, Deentizie = html sonderzeichen zurückübersetzen)
            title = Regex.Replace(title, @"\s+", " ").Trim();
            title = HtmlEntity.DeEntitize(title).ToUpper(); // Erst deentitizen, dann uppercase
            
            link = Regex.Replace(link, @"\s+", "").Trim(); // URL without whitespace

            venue = Regex.Replace(venue, @"\s+", " ").Trim();
            venue = HtmlEntity.DeEntitize(venue);

            date = Regex.Replace(date, @"\s+", " ").Trim();
            date = HtmlEntity.DeEntitize(date);

            time = Regex.Replace(time, @"\s+", " ").Trim();
            time = HtmlEntity.DeEntitize(time);
            
            price = Regex.Replace(price, @"\s+", " ").Trim(); // Neu!
            price = HtmlEntity.DeEntitize(price);
            
            description = Regex.Replace(description, @"</p>|<br\s*/?>", " ", RegexOptions.IgnoreCase);
            var tempNode = HtmlNode.CreateNode(description);
            if (tempNode != null) { description = tempNode.InnerText; } // if node exists, text only
            else { description = ""; } //if empty, empty string
            
            description = HtmlEntity.DeEntitize(description); // deentizize (translate HTML characters back to normal like &amp)
            description = Regex.Replace(description, @"[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27BF]", " "); //remove emojis and weird chars
            description = Regex.Replace(description, @"https?://[^\s]+", "").Trim(); // remove urls in descriptoin text
            description = Regex.Replace(description, @"\s*\[([^\]]+)\]\s*$", "..").Trim(); //remove [] at end
            description = Regex.Replace(description, @"\s+", " ").Trim(); // remove empty space
         
            // 2.3 create new ConcertEvent from scraped element data
            var concertToAdd = new Concert()
            {
                Title = title,
                Genre = "-",
                Date = date,
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
            Console.WriteLine($"Title: {title}, Venue: {venue}, Date: {date}, Time: {time}, Price: {price}, Description: {description}, Link: {link}\n");

        }
        // 3 return concert list
        return concertsClubWakuum;
    }
    
// END CLASS

}