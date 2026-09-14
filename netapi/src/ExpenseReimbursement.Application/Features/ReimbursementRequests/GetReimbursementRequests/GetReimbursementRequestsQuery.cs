using ExpenseReimbursement.Domain.Enums;

namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequests;

/// <summary>Filtros del listado. Ambos son opcionales y se combinan con AND.</summary>
public sealed record GetReimbursementRequestsQuery(
    ReimbursementStatus? Status,
    Guid? EmployeeId);
