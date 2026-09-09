// ==================================================================================
// CLASS: TextCleaner - clean and normalize raw unstructured text from 
// web scrapers
// ==================================================================================

using System.Text.RegularExpressions;
using HtmlAgilityPack;
namespace ConcertsGraz.Utilities;

public static class TextCleaner
{
    
    // ATTRIBUTES
    // CONSTRUCTOR
    // METHODS
    
    // 1 global cleaner (title, venue, date, time, price) - nullable string
    public static string CleanText(string? rawSingleString)
    {
        // 1.1 null/whitespace check
        if (string.IsNullOrWhiteSpace(rawSingleString)) {return"";}
        
        // 1.2 DeEntizize HTML special chars (&amp, &nbsp, etc)
        string deEntSingleString = HtmlEntity.DeEntitize(rawSingleString);
        
        // 1.3 manage empty space ( tab, new line, etc. into 1 whitespace only)
        string cleanString = Regex.Replace(deEntSingleString, @"\s+", " ").Trim();
        
        //1.4 return basic cleaned string
        return cleanString;
    }

    
    
    // 2 description cleaner (description variable) - nullable string
    public static string CleanDescription(string? rawMultiString)
    { 
        // 2.0 null/whitespace check
        if (string.IsNullOrWhiteSpace(rawMultiString)) {return"";}
        
        // 2.1 <p> / <br> durch Leerzeichen ersetzen (verhindert Wortverklebungen)
        string cleanedString = Regex.Replace(rawMultiString, @"</p>|<br\s*/?>", " ", RegexOptions.IgnoreCase);
        
        // 2.2 HTML-Tags via HtmlAgilityPack strippen (InnerText)
        var tempNode = HtmlNode.CreateNode(cleanedString);
        if (tempNode != null) { cleanedString = tempNode.InnerText; } // if node exists, text only
        else { cleanedString = ""; } //if empty, empty string
        
        // 2.3 DeEntitize
        cleanedString= HtmlEntity.DeEntitize(cleanedString); // deentizize (translate HTML characters back to normal like &amp)
        
        // 2.4 Emojis / Sonderzeichen entfernen
        cleanedString = Regex.Replace(cleanedString, @"[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27BF]", " "); //remove emojis and weird chars
        
        // 2.5 URLs in description text entfernen
        cleanedString = Regex.Replace(cleanedString, @"https?://[^\s]+", "").Trim(); // remove urls in descriptoin text
        
        // 2.6 Trimming von eckigen Klammern am Textende ([...])
        cleanedString = Regex.Replace(cleanedString, @"\s*\[([^\]]+)\]\s*$", "..").Trim(); //remove [] at end
        
        //2. 7 manage empty space (tabs etc. plus trim)
        cleanedString = Regex.Replace(cleanedString, @"\s+", " ").Trim(); // remove empty space

        return cleanedString;
    }
    
    
    /*// 3 - scraper-specialized logic IN SCRAPERS not in TextCleaner!:
     Cafe Wolf	Regex-Extraktion von Datum/Zeit aus rawDateTime, fixes &AMP;-Workaround
     PPC	Entfernen von @ 19:00 aus Datumsstring, Strippen von <script>-Blocken
     Alle	.ToUpper() für Titel, URL-spezifisches Whitespace-Löschen (\s+ -> "")
*/
    
    
// END CLASS
}