
using System.Threading;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Tareas_MinimalWebApi.Context;
using Tareas_MinimalWebApi.Entitys;

namespace Tareas_MinimalWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //registrar servicio para  conexion BD
            var connectionString = builder.Configuration.GetConnectionString("Connection");
            builder.Services.AddDbContext<AppDbContext>(
                options => options.UseSqlServer(connectionString)
            );

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapGet("/api/Tareas/Listar", async (AppDbContext context) =>
            {

                var tareas = await context.Tareas.OrderBy(t => t.Orden).ToListAsync();
               // var tareas = await context.Tareas.ToListAsync();
                return tareas;
            });

            
            app.MapGet("/api/Tareas/Obtener/{id:int}", async  Task<Results<NotFound, Ok<Tarea>>> (int id, AppDbContext context) =>
            {
                var tarea = await context.Tareas.FirstOrDefaultAsync( p => p.Id == id);
                if (tarea is null)
                {
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(tarea);
            });

            app.MapPost("/api/Tareas/Crear", async Task<Results<Ok<Tarea>, BadRequest<string>>> (Tarea tarea, AppDbContext context) =>
            {
                try
                {
                    tarea.Estado = "Creada";

                    // Verificar si existen elementos en la tabla Tareas
                    bool tieneTareas = await context.Tareas.AnyAsync();

                    if (!tieneTareas)
                    {
                        tarea.Orden = 1; // Si no hay tareas, asignar 1 como Orden
                    }
                    else
                    {
                        // Obtener Orden a asignar
                        int maxOrden = await context.Tareas.MaxAsync(t => t.Orden);
                        tarea.Orden = maxOrden + 1;
                    }

                    context.Tareas.Add(tarea);
                    await context.SaveChangesAsync();

                    return TypedResults.Ok(tarea);
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest<string>($"Error al crear la tarea: {ex.Message}");
                }
            });


            app.MapPut("/api/Tareas/Editar/{id:int}", async Task<Results<NotFound, NoContent, BadRequest<string>>> (int id, Tarea tarea, AppDbContext context) =>
            {
               if (id != tarea.Id)
               {
                   return TypedResults.BadRequest("Tarea no encontrada..");
               }

               var existeTarea = await context.Tareas.AnyAsync(p => p.Id == id);

               if (!existeTarea)
               {
                   return TypedResults.NotFound();
               }

               context.Update(tarea);
               await context.SaveChangesAsync();
               return TypedResults.NoContent();
            });


            app.MapPut("/api/Tareas/Marcar/{id:int}", async Task<Results<Ok<Tarea>, NotFound, NoContent, BadRequest<string>>> (int id, Tarea tarea, AppDbContext context) =>
            {
                if (id != tarea.Id)
                {
                    return TypedResults.BadRequest("Tarea no encontrada..");
                }

                var existeTarea = await context.Tareas.AnyAsync(p => p.Id == id);

                if (!existeTarea)
                {
                    return TypedResults.NotFound();
                }

                tarea.Estado = "Hecho";

                context.Update(tarea);
                await context.SaveChangesAsync();


                return TypedResults.Ok(new Tarea
                {
                    Id = id,
                    Nombre = tarea.Nombre,
                    Estado = tarea.Estado,
                    Orden = tarea.Orden
                });
              
            });


            app.MapPut("/api/Tareas/ReOrdenar/{id:int}/{nuevoOrden:int}", async Task<Results<Ok<List<Tarea>>, NotFound, NoContent, BadRequest<string>>> (int id, int nuevoOrden, AppDbContext context) =>
            {
                try
                {
                    var tareaAMover = await context.Tareas.FindAsync(id);
                    if (tareaAMover == null)
                    {
                        return TypedResults.NotFound();
                    }

                    // Obtener todas las tareas ordenadas por id de manera ascendente
                    var tareas = await context.Tareas.OrderBy(t => t.Orden).ToListAsync();

                    int ordenActual = tareaAMover.Orden;

                    if (nuevoOrden < ordenActual)
                    {
                        // Mover arriba
                        foreach (var t in tareas.Where(t => t.Orden >= nuevoOrden && t.Orden < ordenActual))
                        {
                            t.Orden++;
                        }
                    }
                    else if (nuevoOrden > ordenActual)
                    {
                        // Mover abajo
                        foreach (var t in tareas.Where(t => t.Orden > ordenActual && t.Orden <= nuevoOrden))
                        {
                            t.Orden--;
                        }
                    }

                    tareaAMover.Orden = nuevoOrden;

                    // Reordenar 
                    tareas = tareas.OrderBy(t => t.Orden).ToList();

                    await context.SaveChangesAsync();


                    var lTareas = await context.Tareas.OrderBy(t => t.Orden).ToListAsync();


                    return TypedResults.Ok(lTareas);


                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest($"Error al ordenar las tareas: {ex.Message}");
                }

            });

            app.MapDelete("/api/Tareas/Eliminar/{id:int}", async  Task <Results <NoContent, NotFound>> (int id, AppDbContext context) =>
            {
                var tarea = await context.Tareas.FindAsync(id);

                if (tarea == null)
                {
                    return TypedResults.NotFound();
                }

                context.Tareas.Remove(tarea);
                await context.SaveChangesAsync();

                return TypedResults.NoContent();

            });


            app.Run();
        }
    }
}
