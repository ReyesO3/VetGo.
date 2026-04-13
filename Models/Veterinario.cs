namespace VetGo.Models
{
    public class Veterinario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Licencia { get; set; }
        // Optional: link to Identity user
        public string IdentityUserId { get; set; }
    }
}