using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetGo.Data;
using VetGo.Models;
using Microsoft.AspNetCore.Authorization;

namespace VetGo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var pendientes = _context.Usuario
                .Where(u => !u.IsApproved && !u.IsRejected)
                .ToList();

            ViewData["TotalMascotas"] = _context.Mascota.Count();
            ViewData["TotalUsers"] = _context.Usuario.Count();

            ViewData["TotalPending"] = pendientes.Count;

            return View(pendientes);
        }

        public async Task<IActionResult> Approved()
        {
            var totalMascotas = await _context.Mascota.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalPending = await _context.Usuario.CountAsync(u => !u.IsApproved && !u.IsRejected);
            var totalApproved = await _context.Usuario.CountAsync(u => u.IsApproved && !u.IsRejected);

            ViewData["TotalMascotas"] = totalMascotas;
            ViewData["TotalUsers"] = totalUsers;
            ViewData["TotalPending"] = totalPending;
            ViewData["TotalApproved"] = totalApproved;

            var approved = await _context.Usuario.Where(u => u.IsApproved && !u.IsRejected).ToListAsync();
            return View(approved);
        }

        public async Task<IActionResult> Rejected()
        {
            var totalMascotas = await _context.Mascota.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalPending = await _context.Usuario.CountAsync(u => !u.IsApproved && !u.IsRejected);
            var totalApproved = await _context.Usuario.CountAsync(u => u.IsApproved && !u.IsRejected);

            ViewData["TotalMascotas"] = totalMascotas;
            ViewData["TotalUsers"] = totalUsers;
            ViewData["TotalPending"] = totalPending;
            ViewData["TotalApproved"] = totalApproved;

            var rejected = await _context.Usuario.Where(u => u.IsRejected).ToListAsync();
            return View(rejected);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _context.Usuario.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsApproved = true;
            user.IsRejected = false;
            _context.Usuario.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var user = await _context.Usuario.FindAsync(id);
            if (user == null)
                return NotFound();

            // Soft reject: mark as rejected and unapprove
            user.IsRejected = true;
            user.IsApproved = false;
            _context.Usuario.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unapprove(int id)
        {
            var user = await _context.Usuario.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsApproved = false;
            _context.Usuario.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Approved));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanently(int id)
        {
            var user = await _context.Usuario.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Usuario.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Rejected));
        }
    }
}