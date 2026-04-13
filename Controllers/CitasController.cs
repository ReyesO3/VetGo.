using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetGo.Data;
using VetGo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;

namespace VetGo.Controllers
{
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Citas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cita.ToListAsync());
        }

        // GET: Citas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Cita = await _context.Cita
                .FirstOrDefaultAsync(m => m.IdCita == id);
            if (Cita == null)
            {
                return NotFound();
            }

            return View(Cita);
        }

        // GET: Citas/Create
        public IActionResult Create(int? veterinarioId)
        {
            // Use ViewModel to provide a typed SelectList to the view
            var vm = new VetGo.ViewModels.CitaCreateViewModel();
            if (veterinarioId.HasValue)
            {
                vm.Cita.Veterinario = veterinarioId.Value;
                var vet = _context.Veterinario.Find(veterinarioId.Value);
                vm.VeterinarioNombre = vet?.Nombre ?? string.Empty;
            }

            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = _context.Usuario.FirstOrDefault(u => u.IdentityUserId == identityUserId);
            var mascotas = appUser != null
                ? _context.Mascota.Where(m => m.IdUsuario == appUser.Id).ToList()
                : new System.Collections.Generic.List<VetGo.Models.Mascota>();

            vm.MascotasSelectList = new SelectList(mascotas, "IdMascota", "Nombre");
            return View(vm);
        }

        // POST: Citas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCita,Fecha,Motivo,Estado,Mascota,Veterinario")] Cita Cita)
        {
            if (ModelState.IsValid)
            {
                _context.Add(Cita);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If model state invalid, rebuild ViewModel with mascotas and vet name to re-render the form
            var vm = new VetGo.ViewModels.CitaCreateViewModel { Cita = Cita };
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = _context.Usuario.FirstOrDefault(u => u.IdentityUserId == identityUserId);
            var mascotas = appUser != null
                ? _context.Mascota.Where(m => m.IdUsuario == appUser.Id).ToList()
                : new System.Collections.Generic.List<VetGo.Models.Mascota>();
            vm.MascotasSelectList = new SelectList(mascotas, "IdMascota", "Nombre");
            return View(vm);
        }

        // GET: Mascotas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Cita = await _context.Cita.FindAsync(id);
            if (Cita == null)
            {
                return NotFound();
            }
            return View(Cita);
        }

        // POST: Cita/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCita,Fecha,Motivo,Estado,Mascota,Veterinario")] Cita Cita)
        {
            if (id != Cita.IdCita)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Cita);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CitaExists(Cita.IdCita))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(Cita);
        }

        // GET: Cita/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Cita = await _context.Cita
                .FirstOrDefaultAsync(m => m.IdCita == id);
            if (Cita == null)
            {
                return NotFound();
            }

            return View(Cita);
        }

        // POST: Cita/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Cita = await _context.Cita.FindAsync(id);
            if (Cita != null)
            {
                _context.Cita.Remove(Cita);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CitaExists(int id)
        {
            return _context.Cita.Any(e => e.IdCita == id);
        }
    }
}
