using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetGo.Data;
using VetGo.Models;

namespace VetGo.Controllers
{
    [Authorize]
    public class HistoriaController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public HistoriaController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Vista para el dueño: solo el dueño puede ver su mascota
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> OwnerDetails(int id) // id = mascotaId
        {
            var identityId = _userManager.GetUserId(User);

            var mascota = await _db.Mascota
                .Include(m => m.Historia_Clinica)
                    .ThenInclude(h => h.Veterinario)
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id && m.Usuario.IdentityUserId == identityId);

            if (mascota == null) return Forbid();

            return View("OwnerDetails", mascota);
        }

        // Vista para el veterinario: solo si el veterinario aparece en alguna entrada de la historia de la mascota
        [Authorize(Roles = "Veterinario")]
        public async Task<IActionResult> VetDetails(int id) // id = mascotaId
        {
            var identityId = _userManager.GetUserId(User);

            var vet = await _db.Veterinario.FirstOrDefaultAsync(v => v.IdentityUserId == identityId);
            if (vet == null) return Forbid();

            var hasAccess = await _db.Historia_Clinica
                .AnyAsync(h => h.MascotaId == id && h.VeterinarioId == vet.Id);

            if (!hasAccess) return Forbid();

            var mascota = await _db.Mascota
                .Include(m => m.Historia_Clinica)
                    .ThenInclude(h => h.Veterinario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mascota == null) return NotFound();

            return View("VetDetails", mascota);
        }
    }
}
