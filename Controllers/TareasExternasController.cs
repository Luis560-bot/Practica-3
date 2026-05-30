using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Practica3.Controllers;

public record ExternalTareaDto(int externalId, string titulo, bool completado);

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
            var todos = await client.GetFromJsonAsync<IEnumerable<JsonElement>>("todos");
            if (todos is null)
                return StatusCode(502, new { Error = "La API externa no respondió correctamente." });

            var mapped = todos.Select(t =>
            {
                var id = t.GetProperty("id").GetInt32();
                var title = t.GetProperty("title").GetString() ?? string.Empty;
                var completed = t.GetProperty("completed").GetBoolean();
                return new ExternalTareaDto(id, title, completed);
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

            var t = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (t.ValueKind == JsonValueKind.Undefined)
                return NotFound();

            var ext = new ExternalTareaDto(
                t.GetProperty("id").GetInt32(),
                t.GetProperty("title").GetString() ?? string.Empty,
                t.GetProperty("completed").GetBoolean()
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
