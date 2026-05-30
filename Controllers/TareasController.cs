using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practica3.Data;
using Practica3.Models;

namespace Practica3.Controllers;

public class TareaDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public EstadoTarea? Estado { get; set; }

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    public PrioridadTarea? Prioridad { get; set; }

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    public DateTime? FechaVencimiento { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class TareasController : ControllerBase
{
    private readonly TareasDbContext _context;

    public TareasController(TareasDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tarea>>> GetTareas(
        [FromQuery] string? estado,
        [FromQuery] string? prioridad,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value.Date > fechaFin.Value.Date)
        {
            return BadRequest(new { Error = "fechaInicio no puede ser mayor que fechaFin." });
        }

        var estadoEnum = EstadoTarea.Pendiente;
        var prioridadEnum = PrioridadTarea.Baja;

        if (!string.IsNullOrWhiteSpace(estado) && !Enum.TryParse<EstadoTarea>(estado, true, out estadoEnum))
        {
            return BadRequest(new { Error = "Estado no válido. Valores permitidos: Pendiente, EnProceso, Completada." });
        }

        if (!string.IsNullOrWhiteSpace(prioridad) && !Enum.TryParse<PrioridadTarea>(prioridad, true, out prioridadEnum))
        {
            return BadRequest(new { Error = "Prioridad no válida. Valores permitidos: Baja, Media, Alta." });
        }

        var query = _context.Tareas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(t => t.Estado == estadoEnum);
        }

        if (!string.IsNullOrWhiteSpace(prioridad))
        {
            query = query.Where(t => t.Prioridad == prioridadEnum);
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(t => t.FechaVencimiento >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(t => t.FechaVencimiento <= fechaFin.Value.Date);
        }

        var tareas = await query.ToListAsync();
        return Ok(tareas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Tarea>> GetTarea(int id)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        return tarea is null ? NotFound() : Ok(tarea);
    }

    [HttpPost]
    public async Task<ActionResult<Tarea>> CreateTarea([FromBody] TareaDto dto)
    {
        if (!ValidarFechaVencimiento(dto.FechaVencimiento))
        {
            ModelState.AddModelError("FechaVencimiento", "La fecha de vencimiento no puede ser menor a la fecha actual.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var tarea = new Tarea
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Estado = dto.Estado!.Value,
            Prioridad = dto.Prioridad!.Value,
            FechaVencimiento = dto.FechaVencimiento!.Value,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Tareas.Add(tarea);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTarea), new { id = tarea.Id }, tarea);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTarea(int id, [FromBody] TareaDto dto)
    {
        if (!ValidarFechaVencimiento(dto.FechaVencimiento))
        {
            ModelState.AddModelError("FechaVencimiento", "La fecha de vencimiento no puede ser menor a la fecha actual.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea is null)
        {
            return NotFound();
        }

        tarea.Titulo = dto.Titulo;
        tarea.Descripcion = dto.Descripcion;
        tarea.Estado = dto.Estado!.Value;
        tarea.Prioridad = dto.Prioridad!.Value;
        tarea.FechaVencimiento = dto.FechaVencimiento!.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTarea(int id)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea is null)
        {
            return NotFound();
        }

        _context.Tareas.Remove(tarea);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static bool ValidarFechaVencimiento(DateTime? fechaVencimiento)
    {
        return fechaVencimiento.HasValue && fechaVencimiento.Value.Date >= DateTime.UtcNow.Date;
    }
}
