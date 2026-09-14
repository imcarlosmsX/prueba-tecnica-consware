namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.RejectReimbursementRequest;

public interface IRejectReimbursementRequestHandler
{
    Task<ReimbursementRequestResponse> HandleAsync(
        RejectReimbursementRequestCommand command,
        CancellationToken cancellationToken);
}
