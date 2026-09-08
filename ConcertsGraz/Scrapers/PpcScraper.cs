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
            // 2.1.1 all info except price (accessible after extra click)
            string title = concert.SelectSingleNode(titleXPath)?.InnerText.Trim().ToUpper() ?? "";
            if (string.IsNullOrWhiteSpace(title)) {continue;} // skip empty nodes
            
            string link = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? "";
            string venue = "PPC Graz";
            string rawDate = concert.SelectSingleNode(dateXPath)?.InnerText.Trim() ?? "";
            string time = concert.SelectSingleNode(timeXPath)?.InnerText.Trim() ?? "";
            string description = concert.SelectSingleNode(descriptionXPath)?.InnerHtml.Trim() ?? "";
            string price = "-";
            
            // 2.1.2 load details page to fetch price 
            if (!string.IsNullOrEmpty(link))
            {
                var detailWeb = new HtmlWeb();
                var detailDoc = detailWeb.Load(link); // load details page url, contains price info
  
                var priceNode = detailDoc.DocumentNode.SelectSingleNode(detailsXPath);
                if (priceNode != null)
                {
                    price = priceNode.InnerText.Trim();
                }
            }

            // 2.2 clean variables (No Whitespace, Deentizie = HTML sonderzeichen zurückübersetzen)
            title = Regex.Replace(title, @"\s+", " ").Trim();
            title = HtmlEntity.DeEntitize(title);

            link = Regex.Replace(link, @"\s+", "").Trim();

            venue = Regex.Replace(venue, @"\s+", " ").Trim();
            venue = HtmlEntity.DeEntitize(venue);

            rawDate = Regex.Replace(rawDate, @"\s*@.*$", "").Trim(); // remove "@ 19:00" from date
            rawDate = Regex.Replace(rawDate, @"\s+", " ").Trim();
            rawDate = HtmlEntity.DeEntitize(rawDate);
            DateTime? parsedDate = DateTimeParser.ParseToDateTime(rawDate);

            time = Regex.Replace(time, @"\s+", " ").Trim();
            time = HtmlEntity.DeEntitize(time);

            price = Regex.Replace(price, @"\s+", " ").Trim();
            price = HtmlEntity.DeEntitize(price);

            description = Regex.Replace(description, @"</p>|<br\s*/?>", " ", RegexOptions.IgnoreCase); // remove HTML tag chars
            description = Regex.Replace(description, @"<script\b[^<]*(?:(?!<\/script>)<script\b[^<]*)*<\/script>", "", RegexOptions.IgnoreCase); // remove JavaScript-Code
            description = Regex.Replace(description, @"window\.\w+[^}]+\}\)", "", RegexOptions.IgnoreCase); // Falls Skript-Tags fehlen

            var tempNode = HtmlNode.CreateNode(description);
            if (tempNode != null) { description = tempNode.InnerText; } 
            else { description = ""; }

            description = HtmlEntity.DeEntitize(description); //html sonderzeichen zurückübersetzen
            description = Regex.Replace(description, @"[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27BF]", " "); // Emojis weg
            description = Regex.Replace(description, @"https?://[^\s]+", "").Trim(); // URLs weg
            description = Regex.Replace(description, @"\s*\[\s*\.{1,3}\s*\]\s*$", "..").Trim(); // replace WordPress-Brackets  [...] or [.] with "..";
            description = Regex.Replace(description, @"\s+", " ").Trim(); //remove empty space

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