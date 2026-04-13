using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetGo.Models;

namespace VetGo.ViewModels
{
    public class CitaCreateViewModel
    {
        public Cita Cita { get; set; } = new Cita();
        public SelectList MascotasSelectList { get; set; } = new SelectList(new List<Mascota>(), "IdMascota", "Nombre");
        public string VeterinarioNombre { get; set; } = string.Empty;
    }
}
