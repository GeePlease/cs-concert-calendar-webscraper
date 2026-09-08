using HtmlAgilityPack;
using System.Text.RegularExpressions;
using ConcertsGraz.Interfaces; // to remove whitespace
using ConcertsGraz.Models;
using ConcertsGraz.Utilities;
namespace ConcertsGraz.Scrapers;

public class CafeWolfScraper : IScraper
{

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
        
        // 2 Filter relevant event (concert) elements via Loop through concert elements and create concert objects
        foreach (var concert in eventElementNodes)
        {
            // 2.1 get raw data (and trim or "cut")
            string rawTitle = concert.SelectSingleNode(titleXPath)?.InnerText.Trim()?? "";
            if (string.IsNullOrWhiteSpace(rawTitle)) {continue;} // skip empty nodes
            
            string link = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? ""; //get link in hidden div (behind "mehr lesen")
            if (string.IsNullOrWhiteSpace(link)) { link = "-"; } // if no link in element on page, -
            
            string description = concert.SelectSingleNode(descriptionXPath)?.InnerText.Trim() ?? "";
            string rawDateTime = concert.SelectSingleNode(dateTimeXPath)?.InnerText.Trim().ToUpper() ?? ""; // get full date + time info
            string venue = "Café Wolf";
            string price = "-";

            // 2.2 clean variables (No Whitespace, Deentizie = HTML sonderzeichen zurückübersetzen, extract detailed data)
            string date = Regex.Match(rawDateTime, @"\d{2}\.\d{2}\.\d{4}").Value; // extract date with regex
            DateTime? parsedDate = DateTimeParser.ParseToDateTime(date); // parse to DateTime Object 
            string time = Regex.Match(rawDateTime, @"\d{2}:\d{2}").Value; // extract time with regex
            
            string title = HtmlEntity.DeEntitize(rawTitle); // decodee HTML entities (like &AMP)
            title = Regex.Replace(title, @"&AMP;", "&", RegexOptions.IgnoreCase); // Fallback, falls Großbuchstaben
            
            title = title.ToUpper(); // Title = uppercasea
            title = Regex.Replace(title, @"\s+", " ").Trim(); // clean whitespace
            
            link = Regex.Replace(link, @"\s+", "").Trim(); // URL without whitespace
            
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
                Date = parsedDate,
                Time = time,
                Venue = venue,
                InfoLink = link,
                Description = description,
                SourceUrl = url,
                Price = price
            };
            
            // 2.4 add new concert element to venue list
            concertsCafeWolf.Add(concertToAdd);
            
            // print elements in console TODO: REMOVE LATER
            Console.WriteLine($"Title: {title}, Venue: {venue}, Date: {parsedDate}, Time: {time}, Price: {price}, Description: {description}, Link: {link}\n");
        }

        return concertsCafeWolf;
    // END CLASS
    }
    




}