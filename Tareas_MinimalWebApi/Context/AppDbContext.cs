using Microsoft.EntityFrameworkCore;
using Tareas_MinimalWebApi.Entitys;

namespace Tareas_MinimalWebApi.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Tarea> Tareas { get; set; }

    }
}
