namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequests;

public interface IGetReimbursementRequestsHandler
{
    Task<IReadOnlyList<ReimbursementRequestResponse>> HandleAsync(
        GetReimbursementRequestsQuery query,
        CancellationToken cancellationToken);
}
