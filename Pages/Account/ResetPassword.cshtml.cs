using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace VetGo.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ResetPasswordModel> _logger;

        public ResetPasswordModel(UserManager<IdentityUser> userManager, ILogger<ResetPasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty, Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Token { get; set; } = string.Empty;

        [BindProperty, Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty, Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public void OnGet(string token = null, string email = null)
        {
            Token = token ?? string.Empty;
            Email = email ?? string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                // No revelar existencia de cuenta
                return RedirectToPage("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Token, Password);
            if (result.Succeeded)
            {
                TempData["ResetSuccess"] = "Contraseña restablecida correctamente. Ya puede iniciar sesión con su nueva contraseña.";
                return RedirectToPage("ResetPasswordConfirmation");
            }

            foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
            return Page();
        }
    }
}