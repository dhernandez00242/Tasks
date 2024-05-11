namespace TareasWebApi.Models
{
    public class Tarea
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Estado { get; set; }
        public int Orden { get; set; }
    }
}
