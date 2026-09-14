namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.CreateReimbursementRequest;

public interface ICreateReimbursementRequestHandler
{
    Task<ReimbursementRequestResponse> HandleAsync(
        CreateReimbursementRequestCommand command,
        CancellationToken cancellationToken);
}
