using System.Text.Json.Serialization;
using ExpenseReimbursement.Api.Middleware;
using ExpenseReimbursement.Application;
using ExpenseReimbursement.Infrastructure;
using ExpenseReimbursement.Infrastructure.Persistence;
using ExpenseReimbursement.Infrastructure.Persistence.Seed;

const string FrontendsCorsPolicy = "AllowFrontends";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Los enums viajan como texto ("Pending", "Meals") y no como números. Un "status": 1
        // obligaría a los dos frontends a mantener su propia tabla de códigos.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// La app móvil corre en un emulador y el panel en otro puerto, así que ambos son de origen
// cruzado. No se usan cookies ni credenciales, por lo que abrir el origen no expone sesiones.
builder.Services.AddCors(options => options.AddPolicy(
    FrontendsCorsPolicy,
    policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Primero en el pipeline: cualquier excepción lanzada más adentro se traduce aquí a
// ProblemDetails, y ningún controller necesita try/catch.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Expense Reimbursement API v1"));

app.UseCors(FrontendsCorsPolicy);

app.MapControllers();

// Aplica las migraciones pendientes y siembra los empleados al arrancar, para que el evaluador
// solo tenga que ejecutar `dotnet run`. En un despliegue real la migración sería un paso
// separado del arranque, para no correrla desde varias instancias a la vez.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseSeeder.SeedAsync(context);
}

app.Run();
