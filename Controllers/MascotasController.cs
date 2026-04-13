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
    public class MascotasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MascotasController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Mascotas
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Get current logged in identity user id
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityUserId))
            {
                return View(new List<Mascota>());
            }

            // Find application user linked to identity user
            var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (appUser == null)
            {
                return View(new List<Mascota>());
            }

            // Return only mascotas that belong to the current application user
            var mascotas = await _context.Mascota
                .Where(m => m.UsuarioId == appUser.Id)
                .Include(m => m.Usuario)
                .AsNoTracking()
                .ToListAsync();
            return View(mascotas);
        }

        // GET: Mascotas/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mascota = await _context.Mascota
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mascota == null)
            {
                return NotFound();
            }

            return View(mascota);
        }

        // GET: Mascotas/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mascotas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("IdMascota,Nombre,Especie,Raza,FechaNacimiento,PesoActual")] Mascota mascota)
        {
            if (!ModelState.IsValid)
                return View(mascota);

            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityUserId))
                return Challenge();

            var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (appUser == null)
            {
                ModelState.AddModelError(string.Empty, "No se encontró el usuario de la aplicación.");
                return View(mascota);
            }

            // Assign mapped FK property
            mascota.UsuarioId = appUser.Id;
            _context.Add(mascota);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Mascotas/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var mascota = await _context.Mascota.FindAsync(id);
            if (mascota == null)
            {
                return NotFound();
            }
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (appUser == null || mascota.UsuarioId != appUser.Id)
                return Forbid();
            return View(mascota);
        }

        // POST: Mascotas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("IdMascota,IdUsuario,Nombre,Especie,Raza,FechaNacimiento,PesoActual")] Mascota mascota)
        {
            if (id != mascota.IdMascota)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
                return View(mascota);

            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (appUser == null || mascota.UsuarioId != appUser.Id)
                return Forbid();

            try
            {
                _context.Update(mascota);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MascotaExists(mascota.IdMascota))
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

        // GET: Mascotas/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var mascota = await _context.Mascota
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mascota == null)
            {
                return NotFound();
            }

            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (appUser == null || mascota.UsuarioId != appUser.Id)
                return Forbid();

            return View(mascota);
        }

        // POST: Mascotas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mascota = await _context.Mascota.FindAsync(id);
            if (mascota == null)
                return NotFound();

            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (appUser == null || mascota.UsuarioId != appUser.Id)
                return Forbid();

            _context.Mascota.Remove(mascota);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MascotaExists(int id)
        {
            return _context.Mascota.Any(e => e.Id == id);
        }
    }
}