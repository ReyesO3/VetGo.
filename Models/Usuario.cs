using System;
using System.Collections.Generic;

namespace VetGo.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        // Link to Identity user
        public string IdentityUserId { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        // Optional professional license (for Veterinario role)
        public string? Licencia { get; set; }

        // Indicates whether the admin approved this account
        public bool IsApproved { get; set; } = false;

        // Indicates whether the admin rejected this account (soft reject)
        public bool IsRejected { get; set; } = false;

        // Navigation: mascotas owned by this usuario
        public virtual ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
    }
}
