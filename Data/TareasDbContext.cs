using Microsoft.EntityFrameworkCore;
using Practica3.Models;

namespace Practica3.Data;

public class TareasDbContext : DbContext
{
    public TareasDbContext(DbContextOptions<TareasDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tarea>()
            .Property(t => t.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<Tarea>()
            .Property(t => t.Prioridad)
            .HasConversion<string>();

        base.OnModelCreating(modelBuilder);
    }
}
