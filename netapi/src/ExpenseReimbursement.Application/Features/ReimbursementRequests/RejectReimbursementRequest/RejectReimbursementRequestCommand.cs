namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.RejectReimbursementRequest;

public sealed record RejectReimbursementRequestCommand(Guid Id, string Reason);
