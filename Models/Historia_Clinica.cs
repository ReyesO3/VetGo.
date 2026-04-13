using System.ComponentModel.DataAnnotations;

namespace VetGo.Models
{
    public class Historia_Clinica
    {
    [Key]
    public int IdHistorial { get; set; }
    // Foreign keys and navigation properties
    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public int VeterinarioId { get; set; }
    public Veterinario? Veterinario { get; set; }

    // Some views/controllers expect `IdCita` and `FechaRegistro` names.
    // Keep both canonical and alias properties so code compiles regardless of naming used elsewhere.
    public int IdCita { get; set; }

    // Primary date property (canonical)
    public DateTime Fecha { get; set; }

    // Alias for older name used in views
    public DateTime FechaRegistro
    {
        get => Fecha;
        set => Fecha = value;
    }

    public string Notas { get; set; } = string.Empty;

    // Diagnóstico y tratamiento
    public string Diagnostico { get; set; } = string.Empty;
    public string Tratamiento { get; set; } = string.Empty;
    }
}
