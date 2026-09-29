namespace ConcertsGraz.ErrorHandling.ScrapingExceptions;

public class EventContainerNotFoundException : Exception
{
    // ATTRIBUTES
    // CONSTRUCTOR
    public EventContainerNotFoundException()
        : base("Main Event/ Concert HTML Node could not be found.")
    {
    }
    
    // METHODS
}