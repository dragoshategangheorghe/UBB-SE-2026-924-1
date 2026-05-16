using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BankApp.Web.Models.Auth;

public class LoginModel
{
    public LoginFormModel LoginForm { get; set; } = new();
}
public class LoginFormModel
{
    [Required]
    [Display(Name = "Email")]
    [EmailAddress]
    public string Email { get; set; } = null !;

    [Required]
    [Display(Name = "Password")]
    [PasswordPropertyText]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; } = false;
}