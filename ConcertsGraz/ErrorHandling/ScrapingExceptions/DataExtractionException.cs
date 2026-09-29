namespace ConcertsGraz.ErrorHandling.ScrapingExceptions;

public class DataExtractionException : Exception
{
    // ATTRIBUTES
    // CONSTRUCTOR
    public DataExtractionException()
        : base("Concert Details could not be extracted.")
    {
    }
    // METHODS
}