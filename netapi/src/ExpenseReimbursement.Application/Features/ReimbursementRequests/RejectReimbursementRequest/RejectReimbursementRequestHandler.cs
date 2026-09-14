using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Application.Common;

namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.RejectReimbursementRequest;

// El motivo obligatorio no se valida aquí sino en ReimbursementRequest.Reject(): así la regla
// se cumple venga la petición de donde venga, no solo de este endpoint.
public sealed class RejectReimbursementRequestHandler : IRejectReimbursementRequestHandler
{
    private readonly IReimbursementRequestRepository _reimbursementRequests;

    public RejectReimbursementRequestHandler(IReimbursementRequestRepository reimbursementRequests)
    {
        _reimbursementRequests = reimbursementRequests;
    }

    public async Task<ReimbursementRequestResponse> HandleAsync(
        RejectReimbursementRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = await _reimbursementRequests.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Reimbursement request", command.Id);

        request.Reject(command.Reason);

        await _reimbursementRequests.SaveChangesAsync(cancellationToken);

        return ReimbursementRequestResponse.From(request);
    }
}
