using System.Text.RegularExpressions;
using ConcertsGraz.Interfaces;
using ConcertsGraz.Models;
using HtmlAgilityPack;

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
            // 2.1.1 all info except price
            string title = concert.SelectSingleNode(titleXPath)?.InnerText.Trim().ToUpper() ?? "";
            if (string.IsNullOrWhiteSpace(title)) {continue;} // skip empty nodes
            
            string link = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? "";
            string venue = "PPC Graz";
            string date = concert.SelectSingleNode(dateXPath)?.InnerText.Trim() ?? "";
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

            date = Regex.Replace(date, @"\s+", " ").Trim();
            date = HtmlEntity.DeEntitize(date);

            time = Regex.Replace(time, @"\s+", " ").Trim();
            time = HtmlEntity.DeEntitize(time);

            price = Regex.Replace(price, @"\s+", " ").Trim();
            price = HtmlEntity.DeEntitize(price);

            description = Regex.Replace(description, @"</p>|<br\s*/?>", " ", RegexOptions.IgnoreCase);
            var tempNode = HtmlNode.CreateNode(description); // TODO: genau verstehen
            if (tempNode != null) { description = tempNode.InnerText; }
            else { description = ""; }

            description = Regex.Replace(description, @"\s+", " ").Trim();
            description = HtmlEntity.DeEntitize(description);

            // 2.3 create new ConcertEvent from scraped element data
            var concertToAdd = new Concert() 
            {
                Title = title,
                Genre = "-", 
                Date = date,
                Time = time,
                Venue = venue,
                Link = link,
                Description = description,
                Url = url,
                Price = price, // Jetzt wird price hier verwendet!
                IsBookmarked = false,
                HasAttended = false 
            };
            
            // 2.4 add new concert element to venue list
            concertsPpc.Add(concertToAdd);
            
            // print elements in console TODO: REMOVE LATER
            Console.WriteLine($"Title: {title}, Venue: {venue}, Date: {date}, Time: {time}, Price: {price}, Link: {link}\n");
        }
        
        // 3 return concerts list
        return concertsPpc;
    }
    
    
}