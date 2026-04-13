using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VetGo.Data;
using VetGo.Models;

namespace VetGo.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Introduce un email");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToPage("ForgotPasswordConfirmation");
            }

            // Generar token y crear enlace (en producción enviar por email)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "", userId = user.Id, code = token },
                protocol: Request.Scheme);

            // En desarrollo guardamos el enlace en TempData para pruebas
            TempData["DevResetLink"] = callbackUrl;

            return RedirectToPage("ForgotPasswordConfirmation");
        }
    }
}