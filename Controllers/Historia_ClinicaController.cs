using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetGo.Data;
using VetGo.Models;

namespace VetGo.Controllers
{
    public class Historia_ClinicaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Historia_ClinicaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Historia_Clinica
        public async Task<IActionResult> Index()
        {
            return View(await _context.Historia_Clinica.ToListAsync());
        }

        // GET: Historia_Clinica/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historia_Clinica = await _context.Historia_Clinica
                .FirstOrDefaultAsync(m => m.IdHistorial == id);
            if (historia_Clinica == null)
            {
                return NotFound();
            }

            return View(historia_Clinica);
        }

        // GET: Historia_Clinica/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Historia_Clinica/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdHistorial,IdCita,Diagnostico,Tratamiento,FechaRegistro")] Historia_Clinica historia_Clinica)
        {
            if (ModelState.IsValid)
            {
                _context.Add(historia_Clinica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(historia_Clinica);
        }

        // GET: Historia_Clinica/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historia_Clinica = await _context.Historia_Clinica.FindAsync(id);
            if (historia_Clinica == null)
            {
                return NotFound();
            }
            return View(historia_Clinica);
        }

        // POST: Historia_Clinica/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdHistorial,IdCita,Diagnostico,Tratamiento,FechaRegistro")] Historia_Clinica historia_Clinica)
        {
            if (id != historia_Clinica.IdHistorial)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(historia_Clinica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Historia_ClinicaExists(historia_Clinica.IdHistorial))
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
            return View(historia_Clinica);
        }

        // GET: Historia_Clinica/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historia_Clinica = await _context.Historia_Clinica
                .FirstOrDefaultAsync(m => m.IdHistorial == id);
            if (historia_Clinica == null)
            {
                return NotFound();
            }

            return View(historia_Clinica);
        }

        // POST: Historia_Clinica/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var historia_Clinica = await _context.Historia_Clinica.FindAsync(id);
            if (historia_Clinica != null)
            {
                _context.Historia_Clinica.Remove(historia_Clinica);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Historia_ClinicaExists(int id)
        {
            return _context.Historia_Clinica.Any(e => e.IdHistorial == id);
        }
    }
}
