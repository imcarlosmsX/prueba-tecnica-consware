using ExpenseReimbursement.Application.Features.Employees.GetEmployees;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseReimbursement.Api.Controllers;

/// <summary>
/// Solo lectura: el enunciado no pide administrar empleados, pero la app móvil necesita
/// seleccionar quién registra el gasto y el panel necesita filtrar por empleado.
/// </summary>
[ApiController]
[Route("api/v1/employees")]
[Produces("application/json")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IGetEmployeesHandler _getEmployeesHandler;

    public EmployeesController(IGetEmployeesHandler getEmployeesHandler)
    {
        _getEmployeesHandler = getEmployeesHandler;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EmployeeResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken) =>
        Ok(await _getEmployeesHandler.HandleAsync(cancellationToken));
}
