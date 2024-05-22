using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using TareasWebApi.Context;
using TareasWebApi.Models;

namespace TareasWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOutputCacheStore _outputCacheStore;

        public TareasController(AppDbContext context, IOutputCacheStore outputCacheStore)
        {
            _context = context;
            _outputCacheStore = outputCacheStore;
        }

        // GET: api/Tareas
        [HttpGet("Listar")]
        [OutputCache(PolicyName = "DesactivarCahe")]
        public async Task<ActionResult<IEnumerable<Tarea>>> GetTareas()
        {
            try
            {
                var tareas = await _context.Tareas.OrderBy(t => t.Orden).ToListAsync();
                return tareas;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las tareas: {ex.Message}");
            }
        }

        // GET: api/Tareas/5
        [HttpGet("Obtener/{id}")]
        [OutputCache(Duration = 5)]
        public async Task<ActionResult<Tarea>> GetTarea(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);

            if (tarea == null)
            {
                return NotFound();
            }

            return tarea;
        }

        // PUT: api/Tareas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Editar/{id}")]

        public async Task<IActionResult> PutTarea(int id, Tarea tarea)
        {
            if (id != tarea.Id)
            {
                return BadRequest();
            }

            _context.Entry(tarea).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await _outputCacheStore.EvictByTagAsync("Cache", default); // Invalidar la cache
            }
            catch (DbUpdateConcurrencyException)
            {

                if (!TareaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }


        [HttpPut("Marcar/{id}")]
        public async Task<ActionResult<Tarea>> MarcarTarea(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
            {
                return NotFound();
            }

            tarea.Estado = "Hecho";
            _context.Tareas.Update(tarea);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync("Cache", default); // Invalidar la cache 

            var tareaDTO = new Tarea
            {
                Id = tarea.Id,
                Nombre = tarea.Nombre,
                Estado = tarea.Estado,
                Orden = tarea.Orden

            };

            return Ok(tareaDTO);
        }

        // POST: api/Tareas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Crear")]
        public async Task<ActionResult<Tarea>> PostTarea(Tarea tarea)
        {
            try
            {
                tarea.Estado = "Creada";

                // Verificar si existen elementos en la tabla Tareas
                bool tieneTareas = await _context.Tareas.AnyAsync();

                if (!tieneTareas)
                {
                    tarea.Orden = 1; // Si no hay tareas, asignar 1 como Orden
                }
                else
                {
                    // Obtener Orden a asignar
                    int maxOrden = await _context.Tareas.MaxAsync(t => t.Orden);
                    tarea.Orden = maxOrden + 1;
                }

                _context.Tareas.Add(tarea);
                await _context.SaveChangesAsync();
                await _outputCacheStore.EvictByTagAsync("Cache", default);

                return CreatedAtAction("GetTarea", new { id = tarea.Id }, tarea);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al agregar la tarea: {ex.Message}");
            }
        }


        // DELETE: api/Tareas/5
        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> DeleteTarea(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
            {
                return NotFound();
            }

            _context.Tareas.Remove(tarea);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync("Cache", default);  // Invalidar la cache

            return NoContent();
        }


        [HttpPut("ReOrdenar/{id}/{nuevoOrden}")]
        public async Task<ActionResult<IEnumerable<Tarea>>> ReOrdenar(int id, int nuevoOrden)
        {
            try
            {

                var tareaAMover = await _context.Tareas.FindAsync(id);
                if (tareaAMover == null)
                {
                    return NotFound();
                }

                // Obtener todas las tareas ordenadas por id de manera ascendente
                var tareas = await _context.Tareas.OrderBy(t => t.Orden).ToListAsync();

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

                await _context.SaveChangesAsync();
                await _outputCacheStore.EvictByTagAsync("Cache", default);  // Invalidar la cache

                return Ok(tareas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al ordenar las tareas: {ex.Message}");
            }
        }


        private bool TareaExists(int id)
        {
            return _context.Tareas.Any(e => e.Id == id);
        }



    }
}
