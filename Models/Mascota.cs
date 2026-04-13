using System.ComponentModel.DataAnnotations;

namespace VetGo.Models
{
    public class Mascota
    {
    [Key]
    public int IdMascota { get; set; }
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string Raza { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public decimal PesoActual { get; set; }

    // Compatibilidad: algunas vistas/controladores esperan `Id`, `Usuario` y `Historia_Clinica`
    // Se exponen aquí como propiedades adicionales que delegan a las propiedades existentes.
    public int Id
    {
        get => IdMascota;
        set => IdMascota = value;
    }

    public int UsuarioId
    {
        get => IdUsuario;
        set => IdUsuario = value;
    }

    public Usuario? Usuario { get; set; }

    // Use ICollection and virtual so EF Core can create relationships and Razor can access them
    public virtual System.Collections.Generic.ICollection<Historia_Clinica> Historia_Clinica { get; set; } = new System.Collections.Generic.List<Historia_Clinica>();

    // Propiedad calculada para la edad (las vistas usan `Edad`)
    public int Edad
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - FechaNacimiento.Year;
            if (FechaNacimiento.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
    }
}
