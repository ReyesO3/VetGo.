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
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class MensajesChatsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;

        public MensajesChatsController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MensajesChats
        public async Task<IActionResult> Index(int? contactId)
        {
            // Determine the current application user from the logged-in Identity user
            var identityId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int currentAppUserId = 0;
            if (!string.IsNullOrEmpty(identityId))
            {
                var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
                if (appUser != null) currentAppUserId = appUser.Id;
            }

            ViewBag.CurrentUserId = currentAppUserId;
            ViewBag.InitialContactId = contactId ?? 0;
            return View(await _context.MensajesChat.ToListAsync());
        }

        // Helper: ensure there's a Usuario record for the current Identity user or veterinarian
        private async Task<int> GetOrCreateUsuarioForIdentityAsync()
        {
            var identityId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId)) return 0;

            var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            if (appUser != null) return appUser.Id;

            // Try veterinarian with same IdentityUserId
            var vet = await _context.Veterinario.FirstOrDefaultAsync(v => v.IdentityUserId == identityId);
            if (vet != null)
            {
                var nuevo = new Usuario
                {
                    IdentityUserId = vet.IdentityUserId,
                    Nombre = vet.Nombre,
                    Email = vet.Email,
                    Rol = "Veterinario",
                    Licencia = vet.Licencia,
                    IsApproved = true
                };
                _context.Usuario.Add(nuevo);
                await _context.SaveChangesAsync();
                return nuevo.Id;
            }

            return 0;
        }

        // GET: MensajesChats/Contacts
        [HttpGet]
        public async Task<IActionResult> Contacts()
        {
            // Return distinct users present in messages as contacts
            // Use Union to produce a translatable query instead of creating an array in the expression
            var userIds = await _context.MensajesChat
                .Select(m => m.IdEmisor)
                .Union(_context.MensajesChat.Select(m => m.IdReceptor))
                .Distinct()
                .ToListAsync();

            var users = await _context.Usuario
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { id = u.Id, name = u.Nombre, email = u.Email })
                .ToListAsync();

            return Json(users);
        }

        // GET: MensajesChats/ContactsForCurrent
        [HttpGet]
        public async Task<IActionResult> ContactsForCurrent()
        {
            var identityId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
                return Unauthorized();

            var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            if (appUser == null)
                return Json(new object[0]);

            var userIds = await _context.MensajesChat
                .Where(m => m.IdEmisor == appUser.Id || m.IdReceptor == appUser.Id)
                .Select(m => m.IdEmisor == appUser.Id ? m.IdReceptor : m.IdEmisor)
                .Distinct()
                .ToListAsync();

            var users = await _context.Usuario
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { id = u.Id, name = u.Nombre, email = u.Email })
                .ToListAsync();

            return Json(users);
        }

        // GET: MensajesChats/Conversation?userId=1&contactId=2
        [HttpGet]
        public async Task<IActionResult> Conversation(int userId, int contactId)
        {
            var messages = await _context.MensajesChat
                .Where(m => (m.IdEmisor == userId && m.IdReceptor == contactId) || (m.IdEmisor == contactId && m.IdReceptor == userId))
                .OrderBy(m => m.FechaEnvio)
                .ToListAsync();

            var identityId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int currentAppUserId = 0;
            if (!string.IsNullOrEmpty(identityId))
            {
                var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
                if (appUser != null) currentAppUserId = appUser.Id;
            }

            var result = messages.Select(m => new
            {
                m.IdMensaje,
                m.IdEmisor,
                m.IdReceptor,
                m.Contenido,
                m.TipoMensaje,
                m.Leido,
                m.FechaEnvio,
                isMine = m.IdEmisor == currentAppUserId
            });

            return Json(result);
        }

        // GET: MensajesChats/ConversationWith?contactId=2
        [HttpGet]
        public async Task<IActionResult> ConversationWith(int contactId)
        {
            var identityId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
                return Unauthorized();

            var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            if (appUser == null)
                return Json(new object[0]);

            var userId = appUser.Id;

            var messages = await _context.MensajesChat
                .Where(m => (m.IdEmisor == userId && m.IdReceptor == contactId) || (m.IdEmisor == contactId && m.IdReceptor == userId))
                .OrderBy(m => m.FechaEnvio)
                .ToListAsync();

            var result = messages.Select(m => new
            {
                m.IdMensaje,
                m.IdEmisor,
                m.IdReceptor,
                m.Contenido,
                m.TipoMensaje,
                m.Leido,
                m.FechaEnvio,
                isMine = m.IdEmisor == userId
            });

            return Json(result);
        }

        // POST: MensajesChats/Send
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] MensajesChat dto)
        {
            if (dto == null)
                return BadRequest();

            // If caller is authenticated, ensure IdEmisor is the current app user
            var identityId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(identityId))
            {
                var appUser = await _context.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
                if (appUser != null)
                {
                    dto.IdEmisor = appUser.Id;
                }
            }

            // Basic validation: receptor must exist
            var receptorExists = await _context.Usuario.AnyAsync(u => u.Id == dto.IdReceptor);
            if (!receptorExists)
            {
                return BadRequest(new { error = "Receptor no existe" });
            }

            dto.FechaEnvio = DateTime.UtcNow;
            _context.MensajesChat.Add(dto);
            await _context.SaveChangesAsync();

            // Notify connected clients via SignalR
            try
            {
                var hub = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.SignalR.IHubContext<VetGo.Hubs.ChatHub>)) as Microsoft.AspNetCore.SignalR.IHubContext<VetGo.Hubs.ChatHub>;
                if (hub != null)
                {
                    // Send to all connected clients in receptor/emisor group names. Use groups named by user id.
                    await hub.Clients.Group(dto.IdReceptor.ToString()).SendCoreAsync("ReceiveMessage", new object[] { new { IdMensaje = dto.IdMensaje, IdEmisor = dto.IdEmisor, IdReceptor = dto.IdReceptor, Contenido = dto.Contenido, FechaEnvio = dto.FechaEnvio } });
                    await hub.Clients.Group(dto.IdEmisor.ToString()).SendCoreAsync("ReceiveMessage", new object[] { new { IdMensaje = dto.IdMensaje, IdEmisor = dto.IdEmisor, IdReceptor = dto.IdReceptor, Contenido = dto.Contenido, FechaEnvio = dto.FechaEnvio } });
                }
            }
            catch
            {
                // non-fatal if SignalR notification fails
            }

            return Json(dto);
        }

        // GET: MensajesChats/UserInfo?id=5
        [HttpGet]
        public async Task<IActionResult> UserInfo(int id)
        {
            var u = await _context.Usuario
                .Where(x => x.Id == id)
                .Select(x => new { id = x.Id, name = x.Nombre, email = x.Email })
                .FirstOrDefaultAsync();

            if (u == null) return NotFound();
            return Json(u);
        }

        // GET: MensajesChats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensajesChat = await _context.MensajesChat
                .FirstOrDefaultAsync(m => m.IdMensaje == id);
            if (mensajesChat == null)
            {
                return NotFound();
            }

            return View(mensajesChat);
        }

        // GET: MensajesChats/Create
        // If a `toId` is provided redirect to Index and open the conversation with that contact
        public IActionResult Create(int? toId)
        {
            if (toId.HasValue)
            {
                return RedirectToAction(nameof(Index), new { contactId = toId.Value });
            }
            return View();
        }

        // POST: MensajesChats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMensaje,IdEmisor,IdReceptor,IdCita,Contenido,TipoMensaje,Leido,FechaEnvio")] MensajesChat mensajesChat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mensajesChat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mensajesChat);
        }

        // GET: MensajesChats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensajesChat = await _context.MensajesChat.FindAsync(id);
            if (mensajesChat == null)
            {
                return NotFound();
            }
            return View(mensajesChat);
        }

        // POST: MensajesChats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMensaje,IdEmisor,IdReceptor,IdCita,Contenido,TipoMensaje,Leido,FechaEnvio")] MensajesChat mensajesChat)
        {
            if (id != mensajesChat.IdMensaje)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mensajesChat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MensajesChatExists(mensajesChat.IdMensaje))
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
            return View(mensajesChat);
        }

        // GET: MensajesChats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensajesChat = await _context.MensajesChat
                .FirstOrDefaultAsync(m => m.IdMensaje == id);
            if (mensajesChat == null)
            {
                return NotFound();
            }

            return View(mensajesChat);
        }

        // POST: MensajesChats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mensajesChat = await _context.MensajesChat.FindAsync(id);
            if (mensajesChat != null)
            {
                _context.MensajesChat.Remove(mensajesChat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MensajesChatExists(int id)
        {
            return _context.MensajesChat.Any(e => e.IdMensaje == id);
        }
    }
}
