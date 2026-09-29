namespace ConcertsGraz.ErrorHandling.ScrapingExceptions;

public class WebsiteLoadingException : Exception
{
    // ATTRIBUTES
    // CONSTRUCTOR
    public WebsiteLoadingException()
        : base("Website could not be loaded.")
    {
    }

    // METHODS
}