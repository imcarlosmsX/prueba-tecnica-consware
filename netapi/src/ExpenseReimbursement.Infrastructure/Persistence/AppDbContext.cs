using ExpenseReimbursement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseReimbursement.Infrastructure.Persistence;

// DECISIÓN DE DISEÑO: el mapeo vive en clases IEntityTypeConfiguration separadas, no en
// atributos sobre las entidades ni en un OnModelCreating gigante.
// POR QUÉ: poner [Column] o [MaxLength] en las entidades metería una dependencia de
// persistencia dentro del Dominio, que debe poder compilarse sin saber que existe una base.
// CONSECUENCIA: el Domain no referencia Entity Framework, y el mapeo de cada tabla se lee
// completo en un archivo.
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ReimbursementRequest> ReimbursementRequests => Set<ReimbursementRequest>();

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
