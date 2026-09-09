using System.Globalization; //process text, numbers and dates culture/ region related
using System.Text.RegularExpressions;
namespace ConcertsGraz.Utilities;

public class DateTimeParser
{
    // ATTRIBUTES
    private static readonly CultureInfo CultureAt = new CultureInfo("de-AT");
    private static readonly CultureInfo CultureEn = new CultureInfo("en-US"); //for english content
    
    // CONSTRUCTOR
    
    // METHODS
    // --------------- METHOD: NAME - parse string -> valid date time object ----------------------
    public static DateTime? ParseToDateTime(string rawDate)
    {
        // return null if date empty
        if (string.IsNullOrWhiteSpace(rawDate))
            return null;

        // 0 basic trim
        string cleanDate = rawDate.Trim();
        
        // 1 Umlaute (ae usw...)
        cleanDate = Regex.Replace(cleanDate, @"\bMaerz\b", "März", RegexOptions.IgnoreCase);
        
        // 2 weekdays and seperation chars (z. B. "Samstag, ", "Fr, ", "Fri | ")
        cleanDate = Regex.Replace(cleanDate, @"^[A-Za-z]{2,9}[\.,\s\|]+", "", RegexOptions.IgnoreCase).Trim();

        // 3 remove unwanted words ("Uhr", "|")
        cleanDate = Regex.Replace(cleanDate, @"\bUhr\b|\|", " ", RegexOptions.IgnoreCase);
        
        // 4 extract and remove time (e.g. "20:00") 
        TimeSpan? extractedTime = null;
        var timeMatch = Regex.Match(cleanDate, @"\b([01]?\d|2[0-3]):([0-5]\d)\b");
        if (timeMatch.Success && TimeSpan.TryParse(timeMatch.Value, out var parsedTime))
        {
            extractedTime = parsedTime;
            cleanDate = cleanDate.Replace(timeMatch.Value, "");
        }
        
        // 5. remove unnecessary punctuation marks and chars
        cleanDate = Regex.Replace(cleanDate, @"[\s,–-]+$", "").Trim();
        cleanDate = Regex.Replace(cleanDate, @"^\s*[\s,–-]+", "").Trim();
        cleanDate = Regex.Replace(cleanDate, @"\s+", " ");
        
        // 6. add year if missing in formats like "12.10."
        if (Regex.IsMatch(cleanDate, @"^\d{1,2}\.\d{1,2}\.$"))
        {
            cleanDate = cleanDate.TrimEnd('.') + $".{DateTime.Now.Year}";
        }
        
        // 7 try to parse into valid date time object
        // multicultural fallback 
        if (DateTime.TryParse(cleanDate, CultureAt, DateTimeStyles.None, out DateTime parsedDate) ||
            DateTime.TryParse(cleanDate, CultureEn, DateTimeStyles.None, out parsedDate) ||
            DateTime.TryParse(cleanDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
        {
            // insert Time in DateTime Object if possible
            if (extractedTime.HasValue)
            {
                parsedDate = parsedDate.Date.Add(extractedTime.Value);
            }

            // force UTC to avoid time difference errors in MongoDB
            return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }       

        return null;
    }
    
// END CLASS
}