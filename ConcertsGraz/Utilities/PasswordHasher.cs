using Microsoft.AspNetCore.Identity;
using ConcertsGraz.Models;

namespace ConcertsGraz.Utilities;

public class PasswordHasher
{
    // ATTRIBUTES
    // official ASP.NET Core PasswordHasher
    private static readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

    // CONSTRUCTOR
    
    // METHODS
    // pw hashing
    public string HashPassword(string password)
    {
        // user hasher
        return _passwordHasher.HashPassword(null, password);
    }
    
    
// END CLASS
}