using System.ComponentModel.DataAnnotations;

namespace VetGo.Models
{
    public class MensajesChat
    {
    [Key]
    public int IdMensaje { get; set; }
    public int IdEmisor { get; set; }
    public int IdReceptor { get; set; }
    public int IdCita { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public string TipoMensaje { get; set; } = string.Empty;
    public int Leido { get; set; }
    public DateTime FechaEnvio { get; set; }
    }
}
