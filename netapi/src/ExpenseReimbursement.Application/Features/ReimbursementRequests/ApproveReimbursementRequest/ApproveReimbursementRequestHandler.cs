using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Application.Common;

namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.ApproveReimbursementRequest;

// El handler no comprueba el estado actual: eso lo hace la entidad, que es quien conoce las
// transiciones válidas. Aquí solo se distingue "no existe" (404) de lo que decida el dominio.
public sealed class ApproveReimbursementRequestHandler : IApproveReimbursementRequestHandler
{
    private readonly IReimbursementRequestRepository _reimbursementRequests;

    public ApproveReimbursementRequestHandler(IReimbursementRequestRepository reimbursementRequests)
    {
        _reimbursementRequests = reimbursementRequests;
    }

    public async Task<ReimbursementRequestResponse> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = await _reimbursementRequests.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Reimbursement request", id);

        request.Approve();

        await _reimbursementRequests.SaveChangesAsync(cancellationToken);

        return ReimbursementRequestResponse.From(request);
    }
}
