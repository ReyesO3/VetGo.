using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetGo.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    // Relax password rules for easier testing/registration; adjust for production as needed
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add session support for server-side CAPTCHA
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Ensure Identity's application cookie is used (sets the login path)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
});

builder.Services.AddAuthorization();

// SignalR for real-time chat
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rutas para controladores MVC (añadido para que Controllers/HistoriaController funcione)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

// Map SignalR hub
app.MapHub<VetGo.Hubs.ChatHub>("/chathub");

// Mantener Razor Pages
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var db = services.GetRequiredService<ApplicationDbContext>();

    // Ensure database and tables are created (useful during development if migrations weren't applied)
    await db.Database.EnsureCreatedAsync();

    string[] roles = { "Admin", "Operador", "Usuario", "Veterinario" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    string email = "admin@VetGo.com";
    string password = "Admin123*";

    var user = await userManager.FindByEmailAsync(email);

    if (user == null)
    {
        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, "Admin");
    }

    // Asegurar que exista un registro en la tabla Usuario para cada IdentityUser
    var allUsers = userManager.Users.ToList();
    foreach (var iu in allUsers)
    {
        // comprobar si ya existe
        var existing = await db.Usuario.FirstOrDefaultAsync(u => u.IdentityUserId == iu.Id);
        if (existing == null)
        {
            var rolesForUser = await userManager.GetRolesAsync(iu);
            var roleName = rolesForUser.FirstOrDefault() ?? "Usuario";
            var isApproved = rolesForUser.Contains("Admin") || rolesForUser.Contains("Veterinario");

            db.Usuario.Add(new VetGo.Models.Usuario
            {
                IdentityUserId = iu.Id,
                Email = iu.Email,
                Nombre = iu.UserName,
                Rol = roleName,
                IsApproved = isApproved
            });
        }
    }

    await db.SaveChangesAsync();
}
app.Run();