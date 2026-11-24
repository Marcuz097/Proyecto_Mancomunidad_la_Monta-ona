#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMontanona.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(UserManager<IdentityUser> userManager,
                         SignInManager<IdentityUser> signInManager,
                         ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty] public InputModel Input { get; set; }
    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();
    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Ingrese su nombre.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Ingrese su correo.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Cree una contraseña.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar para continuar.")]
        public bool Acepto { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim();
        var user = new IdentityUser { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            // Sin rol: el administrador asigna Guardabosque/Admin después.
            await _userManager.AddClaimAsync(user, new Claim("nombre", Input.Nombre.Trim()));
            _logger.LogInformation("Cuenta nueva creada.");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }

        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, Traducir(e));

        return Page();
    }

    // Mensajes en español. El duplicado es genérico para no revelar qué correos existen.
    private static string Traducir(IdentityError e) => e.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => "No pudimos crear la cuenta con esos datos. Si ya te registraste, inicia sesión.",
        "InvalidEmail" or "InvalidUserName" => "El correo no tiene un formato válido.",
        "PasswordTooShort" => "La contraseña es demasiado corta.",
        "PasswordRequiresUpper" => "La contraseña necesita una mayúscula.",
        "PasswordRequiresLower" => "La contraseña necesita una minúscula.",
        "PasswordRequiresDigit" => "La contraseña necesita un número.",
        "PasswordRequiresNonAlphanumeric" => "La contraseña necesita un símbolo (por ejemplo # ! $).",
        _ => "No pudimos crear la cuenta. Revisa los datos e inténtalo de nuevo."
    };
}
