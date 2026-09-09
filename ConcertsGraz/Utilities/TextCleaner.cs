// ==================================================================================
// CLASS: TextCleaner - clean and normalize raw unstructured text from 
// web scrapers
// ==================================================================================

using System.Text.RegularExpressions;
using HtmlAgilityPack;
namespace ConcertsGraz.Utilites;

public static class TextCleaner
{
    
    // ATTRIBUTES
    // CONSTRUCTOR
    // METHODS
    
    // 1- global cleaner (title, venue, date, time, price) - nullable string
    public static string CleanText(string? rawSingleString)
    {
        // null/whitespace check
        if (string.IsNullOrWhiteSpace(rawSingleString)) {return"";}
        
        // DeEntizize HTML special chars (&amp, &nbsp, etc)
        string deEntSingleString = HtmlEntity.DeEntitize(rawSingleString);
        
        // manage empty space ( tab, new line, etc. into 1 whitespace only)
        string cleanString = Regex.Replace(deEntSingleString, @"\s+", " ").Trim();
        
        // return basic cleaned string
        return cleanString;
    }

    
    
    // 2- description cleaner (description variable) - nullable string
    public static string CleanDescription(string? rawMultiString)
    { 
        //HTML-Tags via HtmlAgilityPack strippen (InnerText)
        //DeEntitize
        //Emojis / Sonderzeichen entfernen
        //URLs in description text entfernen
        //Trimming von eckigen Klammern am Textende ([...])
        //manage empty space (tabs etc. plus trim)

        return "";
    }
    
    
    
    /*// 3 - scraper-specialized logic IN SCRAPERS not in TextCleaner!:
     Cafe Wolf	Regex-Extraktion von Datum/Zeit aus rawDateTime, fixes &AMP;-Workaround
     PPC	Entfernen von @ 19:00 aus Datumsstring, Strippen von <script>-Blocken
     Alle	.ToUpper() für Titel, URL-spezifisches Whitespace-Löschen (\s+ -> "")
*/
    
    
    
}