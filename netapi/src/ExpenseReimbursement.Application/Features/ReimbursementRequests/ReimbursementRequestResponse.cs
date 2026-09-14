using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;
using ExpenseReimbursement.Domain.Policies;

namespace ExpenseReimbursement.Application.Features.ReimbursementRequests;

// DECISIÓN DE DISEÑO: mapeo manual con un método estático From(), en vez de AutoMapper.
// POR QUÉ: son doce líneas explícitas que el compilador verifica; AutoMapper las esconde tras
// convenciones que fallan en tiempo de ejecución cuando alguien renombra una propiedad.
// CONSECUENCIA: la forma exacta del JSON que consumen Flutter y Angular se lee en un archivo.
public sealed record ReimbursementRequestResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    decimal Amount,
    string Currency,
    ExpenseCategory Category,
    string Description,
    ReimbursementStatus Status,
    string? RejectionReason,
    DateTime CreatedAtUtc,
    DateTime? DecidedAtUtc)
{
    public static ReimbursementRequestResponse From(ReimbursementRequest request) =>
        From(request, request.Employee?.FullName ?? string.Empty);

    // Sobrecarga para el caso de creación, donde el handler ya consultó el empleado y la
    // propiedad de navegación todavía no está materializada: evita un viaje extra a la base.
    public static ReimbursementRequestResponse From(ReimbursementRequest request, string employeeName) =>
        new(
            request.Id,
            request.EmployeeId,
            employeeName,
            request.Amount,
            ReimbursementPolicy.Currency,
            request.Category,
            request.Description,
            request.Status,
            request.RejectionReason,
            request.CreatedAtUtc,
            request.DecidedAtUtc);
}
