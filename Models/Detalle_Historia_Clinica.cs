using System.ComponentModel.DataAnnotations;

namespace VetGo.Models
{
    public class Detalle_Historia_Clinica
    {
    [Key]
    public int IdDetalle { get; set; }
    public DateTime Fecha { get; set; }
    public string Diagnostico { get; set; } = string.Empty;
    public string Tratamiento { get; set; } = string.Empty;
    public int HistoriaClinica { get; set; }
    public int Veterinario { get; set; }
    public int Cita { get; set; }
    }
}
