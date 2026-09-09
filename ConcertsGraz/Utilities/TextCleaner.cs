// ==================================================================================
// CLASS: TextCleaner - clean and normalize raw unstructured text from 
// web scrapers
// ==================================================================================

namespace ConcertsGraz.Utilites;

public static class TextCleaner
{
    
    // ATTRIBUTES
    // CONSTRUCTOR
    // METHODS
    
    // 1- global cleaner (title, venue, date, time, price)
    public static string CleanText(string rawSingleString)
    {
        // null/whitespace check
        // DeEntizize HTML special chars (&amp, &nbsp, etc)
        // manage empty space ( tab, new line, etc. into 1 whitespace only)
        return "";
    }

    
    
    // 2- description cleaner
    public static string CleanDescription(string rawMultiString)
    {
        // null/whitespace check
        // DeEntizize HTML special chars (&amp, &nbsp, etc)
        // manage empty space ( tab, new line, etc. into 1 whitespace only)
        return "";
    }
    
    
    
    
    // 3 - scraper-specialized logic IN SCRAPERS not in TextCleaner!
    
    
}