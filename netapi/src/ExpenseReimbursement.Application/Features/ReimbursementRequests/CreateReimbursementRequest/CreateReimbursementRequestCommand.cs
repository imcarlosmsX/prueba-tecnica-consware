using ExpenseReimbursement.Domain.Enums;

namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.CreateReimbursementRequest;

public sealed record CreateReimbursementRequestCommand(
    Guid EmployeeId,
    decimal Amount,
    ExpenseCategory Category,
    string Description);
