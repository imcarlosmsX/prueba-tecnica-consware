namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequestById;

public interface IGetReimbursementRequestByIdHandler
{
    Task<ReimbursementRequestResponse> HandleAsync(Guid id, CancellationToken cancellationToken);
}
