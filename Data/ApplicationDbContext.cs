using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetGo.Models;

namespace VetGo.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Mascota> Mascota { get; set; }
        public DbSet<Cita> Cita { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Historia_Clinica> Historia_Clinica { get; set; }
        public DbSet<Veterinario> Veterinario { get; set; }
        public DbSet<MensajesChat> MensajesChat { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure Licencia is optional in the database
            builder.Entity<Usuario>().Property(u => u.Licencia).IsRequired(false);

            // Specify precision/scale for PesoActual to avoid silent truncation
            builder.Entity<Mascota>().Property(m => m.PesoActual).HasPrecision(8, 2);
            // Configure relationship: Mascota.UsuarioId -> Usuario.Id (use strongly-typed FK and collection)
            builder.Entity<Mascota>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.Mascotas)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
