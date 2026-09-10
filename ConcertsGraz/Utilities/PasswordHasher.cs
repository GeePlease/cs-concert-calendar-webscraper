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
     
    // pw verification // 
    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        //Verify pw hash, return = PasswordVerificationResult Enum (0 = failed, 1 = success, 2 = success but old hash)
        var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, providedPassword);

        if (result != 0) return true; 
        return false;
    }


// END CLASS
}