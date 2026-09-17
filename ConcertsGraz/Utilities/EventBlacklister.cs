namespace ConcertsGraz.Utilities;
public class EventBlacklister
{
    // ATTRIBUTES
    private static readonly string[] _ignoredKeywords =
    {
        "afterparty",
        "after party",
        "ausstellung",
        "basar",
        "börse",
        "comedian",
        "comedy",
        "flohmarkt",
        "geschlossene gesellschaft",
        "jam session",
        "jamsession",
        "jam night",
        "karaoke",
        "lesung",
        "messe",
        "open mic",
        "open stage",
        "openmic",
        "poetry slam",
        "pubquiz",
        "quiz",
        "schauspiel",
        "seminar",
        "sommerpause",
        "stand up",
        "stand-up",
        "talk",
        "tanzcafé",
        "turnier",
        "vorlesung",
        "winterpause",
        "workshop",
    };
    
    // CONSTRUCTOR
    
    // METHODS
    public static bool isBlacklisted(string eventTitle)
    {
        if (string.IsNullOrWhiteSpace(eventTitle)) { return true; } //ignore if no title

        foreach (var keywoard in _ignoredKeywords)
        {
            if (eventTitle.Contains(keywoard, StringComparison.OrdinalIgnoreCase)) { return true; } // ignore
        }
        
        return false; // don't ignore
    }

// END CLASS
}
