using System.Globalization; //process text, numbers and dates culture/ region related
using System.Text.RegularExpressions;
namespace ConcertsGraz.Utilities;

public class DateTimeParser
{
    // ATTRIBUTES
    private static readonly CultureInfo CultureAt = new CultureInfo("de-AT");
    
    // CONSTRUCTOR
    
    // METHODS
     // --------------- METHOD: NAME - parse string -> valid date time object ----------------------
     public static DateTime? ParseToDateTime(string rawDate)
     {
         // return null if date empty
         if (string.IsNullOrWhiteSpace(rawDate))
             return null;

         // remove weekday letters (MO, Di, Mi usw.)
         string cleanDate = Regex.Replace(rawDate, @"^[A-Za-z]{2,3}\.\s*", "").Trim();

         // try to parse into valid DateTime Object
         if (DateTime.TryParse(cleanDate, CultureAt, DateTimeStyles.None, out DateTime parsedDate))
         {
             return parsedDate.Date;
         }

         return null;
     }
    
// END CLASS
}