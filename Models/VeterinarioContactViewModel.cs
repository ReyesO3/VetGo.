namespace VetGo.Models
{
    public class VeterinarioContactViewModel
    {
        public int VeterinarioId { get; set; }
        public int UsuarioId { get; set; } // 0 if no Usuario mapping
        public string Nombre { get; set; } = string.Empty;
        public string Licencia { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Disponible { get; set; } = true;
    }
}
