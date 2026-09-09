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
            string? rawTitle = concert.SelectSingleNode(titleXPath)?.InnerText;
            string rawLink = concert.SelectSingleNode(linkXPath)?.GetAttributeValue("href", "") ?? "";
            string? rawDateTime = concert.SelectSingleNode(dateTimeXPath)?.InnerText; //contains both: date and time :/
            string? rawDescription = concert.SelectSingleNode(descriptionXPath)?.InnerHtml; // InnerHtml für HTML-Cleaning
            string rawVenue = "Café Wolf";
            string rawPrice = "-";
            
            // 2.2 Clean variables with TextCleaner
            // 2.2.1 Title, Venue & Link
            string title = TextCleaner.CleanText(rawTitle);
            title = Regex.Replace(title, @"&AMP;", "&", RegexOptions.IgnoreCase); // Cafe Wolf special: Fix uppercase &AMP;
            title = title.ToUpper();
            
            if (string.IsNullOrWhiteSpace(title)) { continue; } // Skip empty nodes
            if (EventBlacklister.isBlacklisted(title)) { continue; } // skip titles that contain non-concert keywoards
            
            string venue = TextCleaner.CleanText(rawVenue);
            string link = Regex.Replace(rawLink, @"\s+", "").Trim();
            if (string.IsNullOrWhiteSpace(link)) { link = "-"; }
            
            // 2.2.2 Cafe Wolf special logic case: Extract date and time from single datetime string
            string cleanedDateTime = TextCleaner.CleanText(rawDateTime);
            string date = Regex.Match(cleanedDateTime, @"\d{2}\.\d{2}\.\d{4}").Value;
            DateTime? parsedDate = DateTimeParser.ParseToDateTime(date);

            string time = Regex.Match(cleanedDateTime, @"\d{2}:\d{2}").Value;
            string price = TextCleaner.CleanText(rawPrice);

            // 2.2.3 Clean description with TextCleaner (description
            string description = TextCleaner.CleanDescription(rawDescription);
            
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