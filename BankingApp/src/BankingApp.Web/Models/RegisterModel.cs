namespace BankingApp.Web.Models;

using System.ComponentModel.DataAnnotations;

public class RegisterModel
{
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _passwordConfirmation = string.Empty;

    [Required]
    [Display(Name = "Full name")]
    public string FullName
    {
        get { return _fullName; }
        set { _fullName = value; }
    }

    [Required]
    [EmailAddress]
    [Display(Name = "Email address")]
    public string Email
    {
        get { return _email; }
        set { _email = value; }
    }

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Password must be at least 8 characters with uppercase, lowercase, a digit and a special character.")]
    [Display(Name = "Password")]
    public string Password
    {
        get { return _password; }
        set { _password = value; }
    }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string PasswordConfirmation
    {
        get { return _passwordConfirmation; }
        set { _passwordConfirmation = value; }
    }
}
