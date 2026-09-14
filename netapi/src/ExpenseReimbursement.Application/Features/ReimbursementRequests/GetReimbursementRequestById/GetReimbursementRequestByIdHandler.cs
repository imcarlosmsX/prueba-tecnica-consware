using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Application.Common;

namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequestById;

public sealed class GetReimbursementRequestByIdHandler : IGetReimbursementRequestByIdHandler
{
    private readonly IReimbursementRequestRepository _reimbursementRequests;

    public GetReimbursementRequestByIdHandler(IReimbursementRequestRepository reimbursementRequests)
    {
        _reimbursementRequests = reimbursementRequests;
    }

    public async Task<ReimbursementRequestResponse> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = await _reimbursementRequests.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Reimbursement request", id);

        return ReimbursementRequestResponse.From(request);
    }
}
