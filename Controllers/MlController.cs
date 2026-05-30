using Microsoft.AspNetCore.Mvc;
using Practica3.Services;

namespace Practica3.Controllers;

public record SentimentRequest(string comentario);
public record SentimentResponse(string comentario, string sentimiento);

[ApiController]
[Route("api/ml")]
public class MlController : ControllerBase
{
    private readonly IMlSentimentService _mlSentimentService;

    public MlController(IMlSentimentService mlSentimentService)
    {
        _mlSentimentService = mlSentimentService;
    }

    [HttpPost("sentimiento")]
    public IActionResult AnalizarSentimiento([FromBody] SentimentRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.comentario))
        {
            return BadRequest(new { Error = "El campo 'comentario' es obligatorio y no puede estar vacío." });
        }

        // Predict the sentiment using our ML.NET Service
        var sentimiento = _mlSentimentService.PredictSentiment(request.comentario);

        // Map the result to the requested format
        var response = new SentimentResponse(request.comentario, sentimiento);
        
        return Ok(response);
    }
}
