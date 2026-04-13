using System.ComponentModel.DataAnnotations;

namespace VetGo.Models
{
    public class Cita
    {
    [Key]
    public int IdCita { get; set; }
    public DateTime Fecha { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int Mascota { get; set; }
    public int Veterinario { get; set; }
    }
}
