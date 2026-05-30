# Practica 3 - API de Tareas

API RESTful para gestionar tareas con ASP.NET Core y SQLite.

## Endpoints

- `GET /api/tareas`
- `GET /api/tareas/{id}`
- `POST /api/tareas`
- `PUT /api/tareas/{id}`
- `DELETE /api/tareas/{id}`

## Modelo Tarea

- `Id`
- `Titulo`
- `Descripcion`
- `Estado` (Pendiente, EnProceso, Completada)
- `Prioridad` (Baja, Media, Alta)
- `FechaCreacion`
- `FechaVencimiento`

## Ejecutar localmente

```powershell
cd Practica3
dotnet restore
dotnet run
```

## Comandos de migración

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Filtros disponibles

`GET /api/tareas` acepta filtros opcionales:

- `estado`: Pendiente, EnProceso, Completada
- `prioridad`: Baja, Media, Alta
- `fechaInicio`: fecha mínima de vencimiento
- `fechaFin`: fecha máxima de vencimiento

Ejemplos:

- `GET /api/tareas?estado=Pendiente`
- `GET /api/tareas?prioridad=Alta`
- `GET /api/tareas?fechaInicio=2026-05-01&fechaFin=2026-05-31`

## Validaciones principales

- `Titulo` es obligatorio.
- `Estado` es obligatorio.
- `Prioridad` es obligatorio.
- `FechaVencimiento` no puede ser menor a la fecha actual.

## Observaciones

- Se usa SQLite con `Data Source=tareas.db`.
- Se crea automáticamente la base de datos al iniciar la aplicación.

## Consumo API externa

Se añade el endpoint para consumir `https://jsonplaceholder.typicode.com/todos`:

- `GET /api/tareas-externas` — devuelve la lista mapeada.
- `GET /api/tareas-externas/{id}` — devuelve tarea externa por id.

Respuesta mapeada:

```
{
	"externalId": 1,
	"titulo": "delectus aut autem",
	"completado": false
}
```

Validaciones:

- Si la API externa no responde devuelve `502` o `504` según el error.
- Si el ID no existe devuelve `404`.
- Se mapea el JSON externo a un DTO propio antes de devolverlo.
# Practica-3
