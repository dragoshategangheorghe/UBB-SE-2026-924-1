using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BankApp.Web.Models.Auth;

public class LoginModel
{
    public LoginFormModel Login { get; set; } = new();
}
public class LoginFormModel
{
    [Required]
    [Display(Name = "Email")]
    [EmailAddress]
    public string Email { get; set; } = null !;

    [Required]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; } = false;
    public string LoginState { get; set; } = null!;
}