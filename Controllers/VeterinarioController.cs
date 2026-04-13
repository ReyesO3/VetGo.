using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGo.Data;
using VetGo.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace VetGo.Controllers
{
    [Authorize(Roles = "Veterinario")]
    public class VeterinarioController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VeterinarioController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET: /Veterinario
        public IActionResult Index()
        {
            return View();
        }

        // Placeholder actions for links in the view. Implement views as needed.
        public async Task<IActionResult> Citas()
        {
            var today = DateTime.Today;

            var q = from c in _db.Cita
                    join m in _db.Mascota on c.Mascota equals m.IdMascota
                    join u in _db.Usuario on m.IdUsuario equals u.Id into uu
                    from u in uu.DefaultIfEmpty()
                    where c.Fecha.Date == today
                    select new VeterinarioCitaViewModel
                    {
                        IdCita = c.IdCita,
                        Fecha = c.Fecha,
                        Motivo = c.Motivo,
                        Estado = c.Estado,
                        MascotaId = m.IdMascota,
                        MascotaNombre = m.Nombre,
                        MascotaEspecie = m.Especie,
                        PropietarioNombre = u != null ? u.Nombre : string.Empty
                    };

            var citas = await q.OrderBy(x => x.Fecha).ToListAsync();
            return View("Citas", citas);
        }

        public IActionResult Pacientes()
        {
            // Render the Pacientes view (list of patients for the veterinarian)
            return View("Pacientes");
        }

        public IActionResult Mensajes()
        {
            // Show veterinarian-specific messaging UI
            return View("Mensajes");
        }

        public IActionResult Videollamada()
        {
            return View();
        }
    }
}
