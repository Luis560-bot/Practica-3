using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Practica3.Controllers;

public record ExternalTareaDto(int externalId, string titulo, bool completado);

internal sealed class ExternalTodoApiItem
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public bool Completed { get; set; }
}

[ApiController]
[Route("api/tareas-externas")]
public class TareasExternasController : ControllerBase
{
    private readonly IHttpClientFactory _factory;

    public TareasExternasController(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<IActionResult> GetTareasExternas()
    {
        var client = _factory.CreateClient("JsonPlaceholder");
        try
        {
            var todos = await client.GetFromJsonAsync<List<ExternalTodoApiItem>>("todos");
            if (todos is null)
                return StatusCode(502, new { Error = "La API externa no respondió correctamente." });

            var mapped = todos.Select(t =>
            {
                return new ExternalTareaDto(t.Id, t.Title ?? string.Empty, t.Completed);
            });

            return Ok(mapped);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new { Error = "Error al conectar con la API externa." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, new { Error = "Tiempo de espera agotado al llamar a la API externa." });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTareaExterna(int id)
    {
        var client = _factory.CreateClient("JsonPlaceholder");
        try
        {
            var response = await client.GetAsync($"todos/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound();

            response.EnsureSuccessStatusCode();

            var t = await response.Content.ReadFromJsonAsync<ExternalTodoApiItem>();
            if (t is null)
                return NotFound();

            var ext = new ExternalTareaDto(
                t.Id,
                t.Title ?? string.Empty,
                t.Completed
            );

            return Ok(ext);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new { Error = "Error al conectar con la API externa." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, new { Error = "Tiempo de espera agotado al llamar a la API externa." });
        }
    }
}
