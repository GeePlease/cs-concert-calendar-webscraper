using System.Text.RegularExpressions;
namespace ConcertsGraz.Utilities;

// ==================================================================================
// CLASS: Input Validator for fields Username, Email, Password.
/*
→ null/whitespace?
→ trim
→ 3–50?

Email
→ null/whitespace?
→ trim
→ Regex?

Password
→ null/empty?
→ 8–50?
→ mindestens eine Ziffer?
→ mindestens ein Buchstabe?*/
// ==================================================================================

public static class InputValidator
{
    // ATTRIBUTES
    // CONSTRUCTOR
    // METHODS
    
    // --- method username validation
    // requ: null check, string len between 3 - 50, trim!
    public static bool ValidateUsername(string? providedUsername)
    
    {
        if(string.IsNullOrWhiteSpace(providedUsername)) {return false;}
        providedUsername = providedUsername.Trim();
        if (providedUsername.Length < 3 || providedUsername.Length > 50) {return false;}
        return true;
    }
    
    
    // --- method email validation
    // requ: null check, email format  regex at + global, trim!
    public static bool ValidateEmail(string? providedEmail) {
        
        if(string.IsNullOrWhiteSpace(providedEmail)) {return false;}
        providedEmail = providedEmail.Trim();
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(providedEmail, emailPattern)) { return false; }

        return true;
    }
    
    //---  method pw validation
    // requ: null check, string len between 8-50, min 1 zahl + min 1 buchstabe
    public static bool ValidatePassword(string? providedPassword) 
 
    {
        if(string.IsNullOrEmpty(providedPassword)) {return false;}
        
        if(providedPassword.Length < 8 || providedPassword.Length > 50) {return false;}
        if (!providedPassword.Any(char.IsDigit) ||
            !providedPassword.Any(char.IsLetter))
        {
            return false;
        }

        return true;
    }
    
    
    

//END CLASS
}