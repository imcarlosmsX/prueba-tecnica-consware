using ExpenseReimbursement.Api.Contracts;
using ExpenseReimbursement.Application.Features.ReimbursementRequests;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.ApproveReimbursementRequest;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.CreateReimbursementRequest;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequestById;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequests;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.RejectReimbursementRequest;
using ExpenseReimbursement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseReimbursement.Api.Controllers;

// DECISIÓN DE DISEÑO: aprobar y rechazar son POST sobre un sub-recurso con nombre de verbo
// (/{id}/approve), no un PUT sobre la solicitud completa.
// POR QUÉ: un PUT significa "sustituye este recurso por el que te envío", lo que invitaría al
// cliente a mandar un Status y decidir él el estado. El POST expresa una intención de negocio y
// deja claro que quien decide el estado resultante es el servidor.
// CONSECUENCIA: el frontend no puede fijar un estado arbitrario ni siquiera por accidente.
[ApiController]
[Route("api/v1/reimbursement-requests")]
[Produces("application/json")]
public sealed class ReimbursementRequestsController : ControllerBase
{
    private readonly ICreateReimbursementRequestHandler _createHandler;
    private readonly IGetReimbursementRequestsHandler _getAllHandler;
    private readonly IGetReimbursementRequestByIdHandler _getByIdHandler;
    private readonly IApproveReimbursementRequestHandler _approveHandler;
    private readonly IRejectReimbursementRequestHandler _rejectHandler;

    public ReimbursementRequestsController(
        ICreateReimbursementRequestHandler createHandler,
        IGetReimbursementRequestsHandler getAllHandler,
        IGetReimbursementRequestByIdHandler getByIdHandler,
        IApproveReimbursementRequestHandler approveHandler,
        IRejectReimbursementRequestHandler rejectHandler)
    {
        _createHandler = createHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
    }

    /// <summary>Registra una solicitud de reembolso. Las de $500.000 COP o menos nacen aprobadas.</summary>
    [HttpPost]
    [ProducesResponseType<ReimbursementRequestResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateReimbursementRequestDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateReimbursementRequestCommand(
            dto.EmployeeId, dto.Amount, dto.Category, dto.Description);

        var response = await _createHandler.HandleAsync(command, cancellationToken);

        // CreatedAtRoute y no CreatedAtAction: MVC recorta el sufijo "Async" de los nombres de
        // acción, así que buscar la acción por nameof(GetByIdAsync) falla en tiempo de ejecución.
        // El nombre de ruta declarado en el [HttpGet] es explícito y sobrevive a un renombrado.
        return CreatedAtRoute(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    /// <summary>Lista las solicitudes, opcionalmente filtradas por estado y por empleado.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ReimbursementRequestResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] ReimbursementStatus? status,
        [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken)
    {
        var query = new GetReimbursementRequestsQuery(status, employeeId);

        return Ok(await _getAllHandler.HandleAsync(query, cancellationToken));
    }

    /// <summary>Devuelve el detalle de una solicitud.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetByIdAsync))]
    [ProducesResponseType<ReimbursementRequestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await _getByIdHandler.HandleAsync(id, cancellationToken));

    /// <summary>Aprueba una solicitud Pendiente.</summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType<ReimbursementRequestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApproveAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await _approveHandler.HandleAsync(id, cancellationToken));

    /// <summary>Rechaza una solicitud Pendiente. El motivo es obligatorio.</summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType<ReimbursementRequestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectAsync(
        Guid id,
        [FromBody] RejectReimbursementRequestDto dto,
        CancellationToken cancellationToken)
    {
        var command = new RejectReimbursementRequestCommand(id, dto.Reason);

        return Ok(await _rejectHandler.HandleAsync(command, cancellationToken));
    }
}
