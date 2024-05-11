using Microsoft.EntityFrameworkCore;
using TareasWebApi.Models;

namespace TareasWebApi.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Tarea> Tareas { get; set; }
    
    }
}
