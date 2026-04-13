using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using VetGo.Data;
using VetGo.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace VetGo.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public LoginController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = context;
            _env = env;
        }

        // GET: /Login
        [AllowAnonymous]
        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // GET: /Login/Veterinario
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Veterinario()
        {
            // Mostrar la vista de inicio de sesión para veterinarios
            return View();
        }

        // GET: /Login/CrearVeterinario
        [HttpGet]
        [AllowAnonymous]
        public IActionResult CrearVeterinario()
        {
            // Mostrar la vista de registro para veterinarios
            return View("CrearVeterinario");
        }

        // GET: /Login/Admin
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Admin()
        {
            return View();
        }

        // GET: /Login/Dueno
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Dueno()
        {
            return View();
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string email, string password, string role = null, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            #nullable enable
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Ingrese email y contraseña.";
                return View();
            }

            // Find user by email first
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "Credenciales incorrectas.";
                return View();
            }

            // Validate password first without signing in
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                ViewBag.Error = "Credenciales incorrectas.";
                return View();
            }

            // Get roles for the identity user
            var identityRoles = await _userManager.GetRolesAsync(user);

            // NOTE: Do not allow automatic sign-in for any role. All application users
            // must be approved in the `Usuario` table before signing in.

            // Ensure there's an application user record and require approval for ALL users
            var appUser = await _db.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);
            if (appUser == null)
            {
                var roleNameAuto = identityRoles.FirstOrDefault() ?? "Usuario";
                appUser = new Usuario
                {
                    IdentityUserId = user.Id,
                    Nombre = user.UserName?.Split('@')[0] ?? user.Email,
                    Email = user.Email,
                    Rol = roleNameAuto,
                    IsApproved = false,
                    IsRejected = false
                };
                _db.Usuario.Add(appUser);
                await _db.SaveChangesAsync();
            }

            // Block login for rejected accounts
            if (appUser.IsRejected)
            {
                ModelState.AddModelError(string.Empty, "Tu cuenta fue rechazada por el administrador");
                return View();
            }

            // Block login for not yet approved accounts (this enforces approval for ALL roles)
            if (!appUser.IsApproved)
            {
                ModelState.AddModelError(string.Empty, "Tu cuenta está pendiente de aprobación");
                return View();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // At this point the user is approved; sign in and redirect by role
            await _signInManager.SignInAsync(user, isPersistent: false);

            var roles = identityRoles;
            // If a specific role was requested from the login form, validate it
            if (!string.IsNullOrEmpty(role))
            {
                if (!roles.Contains(role))
                {
                    await _signInManager.SignOutAsync();
                    ViewBag.Error = $"El usuario no tiene el rol '{role}'.";
                    return View();
                }
            }

            if (roles.Contains("Admin")) 
                return RedirectToAction("Index", "Admin"); 
            if (roles.Contains("Veterinario")) 
                return RedirectToAction("Index", "Veterinario"); 
            if (roles.Contains("Operador")) 
                return RedirectToAction("Index", "Historia_Clinica"); 
            if (roles.Contains("Usuario")) 
                return RedirectToAction("Index", "Mascotas"); 

            // Default fallback 
            return RedirectToAction("Index", "Home"); 


            ViewBag.Error = "Credenciales incorrectas.";
            return View();
        }

        // GET: /Login/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new VetGo.Models.RegisterViewModel { Rol = "Usuario" });
        }

        // POST: /Login/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(VetGo.Models.RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // return view with validation messages
                return View(model);
            }

            var nombre = model.Nombre;
            var email = model.Email;
            var password = model.Password;
            var licencia = model.Licencia;
            var rol = model.Rol ?? "Usuario";

            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Ensure the requested role exists and add user to it
                var roleName = rol;
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
                // Add user to the requested role only
                await _userManager.AddToRoleAsync(user, roleName);

                // Create application user entity linked to Identity user
                var usuario = new Usuario
                {
                    IdentityUserId = user.Id,
                    Nombre = nombre,
                    Email = email,
                    Rol = roleName,
                    Licencia = roleName == "Veterinario" ? licencia : null,
                    // By default new accounts are not approved. Admin must approve in production.
                    IsApproved = (_env.IsDevelopment() && roleName == "Usuario") ? true : false,
                    IsRejected = false
                };
                _db.Usuario.Add(usuario);
                await _db.SaveChangesAsync();

                // Set TempData to show message on login page and prefill email
                TempData["RegisterMessage"] = "Registro exitoso. Su cuenta está pendiente de aprobación por un administrador.";
                TempData["RegisteredEmail"] = email;

                // In development, allow immediate sign in for convenience when auto-approved
                if (_env.IsDevelopment() && usuario.IsApproved)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    if (roleName == "Usuario")
                        return RedirectToAction("Index", "Mascotas");
                    if (roleName == "Veterinario")
                        return RedirectToAction("Index", "Veterinario");
                    if (roleName == "Admin")
                        return RedirectToAction("Index", "Admin");
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Index", "Login");
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(model);
        }

        // POST: /Login/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }
    }
}