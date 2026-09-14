namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.ApproveReimbursementRequest;

public interface IApproveReimbursementRequestHandler
{
    Task<ReimbursementRequestResponse> HandleAsync(Guid id, CancellationToken cancellationToken);
}
