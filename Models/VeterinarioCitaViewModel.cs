using System;

namespace VetGo.Models
{
    public class VeterinarioCitaViewModel
    {
        public int IdCita { get; set; }
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int MascotaId { get; set; }
        public string MascotaNombre { get; set; } = string.Empty;
        public string MascotaEspecie { get; set; } = string.Empty;
        public string PropietarioNombre { get; set; } = string.Empty;
    }
}
