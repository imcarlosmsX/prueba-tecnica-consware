using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Infrastructure.Persistence;
using ExpenseReimbursement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseReimbursement.Infrastructure;

// DECISIÓN DE DISEÑO: el motor de base de datos se elige en esta única línea, y el resto del
// sistema solo conoce las interfaces de repositorio que declara la capa de Aplicación.
// POR QUÉ: es la prueba práctica de que la regla de dependencias sirve para algo. Migrar a SQL
// Server o PostgreSQL es cambiar UseSqlite por UseSqlServer y la cadena de conexión.
// CONSECUENCIA: ni el Dominio ni los casos de uso ni los controllers saben qué motor hay debajo.
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is missing from configuration.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IReimbursementRequestRepository, ReimbursementRequestRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        return services;
    }
}
