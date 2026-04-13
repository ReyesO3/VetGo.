using System.Collections.Generic;

namespace VetGo.Models
{
    public static class Datos
    {
        public static List<Usuario> Usuarios { get; } = new List<Usuario>()
        {
            new Usuario { Nombre = "Admin", Email = "admin@vetgo.com", Rol = "Administrador" },
            new Usuario { Nombre = "Vet", Email = "vet@vetgo.com", Rol = "Veterinario" },
            new Usuario { Nombre = "User", Email = "user@vetgo.com", Rol = "Usuario" }
        };
    }
}
