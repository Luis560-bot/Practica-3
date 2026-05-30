using Microsoft.EntityFrameworkCore;
using Practica3.Data;
using Practica3.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient("JsonPlaceholder", client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TareasDbContext>(options =>
    options.UseSqlite("Data Source=tareas.db"));

// Register ML.NET Sentiment Analysis Service
builder.Services.AddSingleton<IMlSentimentService, MlSentimentService>();

var app = builder.Build();

// Habilitar Swagger y Swagger UI en /swagger-ui/ y dejar el JSON en /swagger/v1/swagger.json
app.UseSwagger(c =>
{
    // Ruta por defecto para el JSON de OpenAPI
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    // Asegurarse de apuntar al endpoint correcto del JSON
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Practica 3 API V1");
    // Colocar la UI en /swagger-ui
    c.RoutePrefix = "swagger-ui";
});

app.MapGet("/", () => Results.Text(@"<html><body><h1>API Practica 3</h1><ul><li><a href='/api/tareas'>/api/tareas</a></li><li><a href='/api/tareas-externas'>/api/tareas-externas</a></li><li><a href='/api/tareas-externas/1'>/api/tareas-externas/1</a></li><li>POST <code>/api/ml/sentimiento</code> (Análisis de sentimiento con ML.NET)</li><li><a href='/openapi'>/openapi</a> (JSON OpenAPI)</li><li><a href='/swagger'>/swagger</a> (Swagger UI)</li></ul></body></html>", "text/html"));

app.MapGet("/openapi", () => Results.Json(new
{
    title = "Practica 3 API",
    endpoints = new[]
    {
        new { method = "GET", path = "/api/tareas" },
        new { method = "GET", path = "/api/tareas/{id}" },
        new { method = "POST", path = "/api/tareas" },
        new { method = "PUT", path = "/api/tareas/{id}" },
        new { method = "DELETE", path = "/api/tareas/{id}" },
        new { method = "GET", path = "/api/tareas-externas" },
        new { method = "GET", path = "/api/tareas-externas/{id}" },
        new { method = "POST", path = "/api/ml/sentimiento" }
    }
}));

app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TareasDbContext>();
    db.Database.Migrate();
}

app.Run();
