using ExpenseReimbursement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseReimbursement.Infrastructure.Persistence.Seed;

// DECISIÓN DE DISEÑO: los empleados se siembran al arrancar con identificadores fijos, y la
// siembra no hace nada si la tabla ya tiene filas.
// POR QUÉ: el enunciado permite identificar al empleado "de forma sencilla" y no pide un CRUD
// de empleados, pero sí exige filtrar por empleado. Ids fijos permiten documentarlos en el
// README y que la app móvil los seleccione sin inventar datos.
// CONSECUENCIA: arrancar la API dos veces no duplica empleados.
public static class DatabaseSeeder
{
    private static readonly (Guid Id, string FullName)[] Employees =
    [
        (Guid.Parse("11111111-1111-1111-1111-111111111111"), "Carlos Mendoza"),
        (Guid.Parse("22222222-2222-2222-2222-222222222222"), "Laura Gutiérrez"),
        (Guid.Parse("33333333-3333-3333-3333-333333333333"), "Andrés Rojas")
    ];

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Employees.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var (id, fullName) in Employees)
        {
            await context.Employees.AddAsync(Employee.Create(id, fullName), cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
