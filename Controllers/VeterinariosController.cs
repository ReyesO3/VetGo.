using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using VetGo.Data;
using VetGo.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace VetGo.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class VeterinariosController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VeterinariosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Veterinarios
        public async Task<IActionResult> Index()
        {
            // Build a view model that includes the linked Usuario.Id when available so chat links point to Usuario
            var vets = await _db.Veterinario
                .Select(v => new VeterinarioContactViewModel
                {
                    VeterinarioId = v.Id,
                    Nombre = v.Nombre,
                    Email = v.Email,
                    Licencia = v.Licencia,
                    Disponible = true,
                    UsuarioId = _db.Usuario.Where(u => u.IdentityUserId == v.IdentityUserId).Select(u => (int?)u.Id).FirstOrDefault() ?? 0
                })
                .ToListAsync();
            return View(vets);
        }
    }
}
