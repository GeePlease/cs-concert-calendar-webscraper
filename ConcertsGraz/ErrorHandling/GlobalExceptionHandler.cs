using Microsoft.AspNetCore.Diagnostics;
using MongoDB.Driver;

namespace ConcertsGraz.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    // ATTRIBUTES
    private readonly ILogger<GlobalExceptionHandler> _logger;
    
    // CONSTRUCTOR
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    // METHODS
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // --- LOGGING
        // Log technical exception details for development/debugging in console
        // Exception details are NOT sent to the frontend
        _logger.LogError(
            exception,
            "Unhandled exception occurred.");

        // --- DATABASE EXCEPTIONS
        if (exception is MongoException)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status503ServiceUnavailable;
        }
    
        // --- GENERAL / UNHANDLED EXCEPTIONS
        else
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status500InternalServerError;
        }

        // --- RESPONSE
        // Same general message for every unexpected server error.
        // Technical exception details are not sent to the frontend.
        await httpContext.Response.WriteAsJsonAsync(
            new { message = "Ein unerwarteter Serverfehler ist aufgetreten." },
            cancellationToken);

        // Exception handled
        return true;
    }
    
// END CLASS
}