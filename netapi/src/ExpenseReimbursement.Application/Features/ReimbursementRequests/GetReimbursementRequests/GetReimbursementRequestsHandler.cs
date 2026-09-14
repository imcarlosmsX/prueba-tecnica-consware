using ExpenseReimbursement.Application.Abstractions;

namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequests;

public sealed class GetReimbursementRequestsHandler : IGetReimbursementRequestsHandler
{
    private readonly IReimbursementRequestRepository _reimbursementRequests;

    public GetReimbursementRequestsHandler(IReimbursementRequestRepository reimbursementRequests)
    {
        _reimbursementRequests = reimbursementRequests;
    }

    public async Task<IReadOnlyList<ReimbursementRequestResponse>> HandleAsync(
        GetReimbursementRequestsQuery query,
        CancellationToken cancellationToken)
    {
        var requests = await _reimbursementRequests.ListAsync(
            query.Status,
            query.EmployeeId,
            cancellationToken);

        return requests.Select(ReimbursementRequestResponse.From).ToList();
    }
}
