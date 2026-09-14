using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Application.Common;
using ExpenseReimbursement.Domain.Entities;

namespace ExpenseReimbursement.Application.Features.ReimbursementRequests.CreateReimbursementRequest;

// DECISIÓN DE DISEÑO: el handler solo comprueba que el empleado referenciado existe y delega
// toda la validación de campos y la política de aprobación automática a la entidad.
// POR QUÉ: si el handler decidiera el estado inicial, la regla central quedaría fuera del
// dominio y habría que replicarla en cualquier otro punto de entrada (un job, una importación).
// CONSECUENCIA: este archivo no menciona el umbral ni el estado inicial; solo orquesta.
public sealed class CreateReimbursementRequestHandler : ICreateReimbursementRequestHandler
{
    private readonly IReimbursementRequestRepository _reimbursementRequests;
    private readonly IEmployeeRepository _employees;

    public CreateReimbursementRequestHandler(
        IReimbursementRequestRepository reimbursementRequests,
        IEmployeeRepository employees)
    {
        _reimbursementRequests = reimbursementRequests;
        _employees = employees;
    }

    public async Task<ReimbursementRequestResponse> HandleAsync(
        CreateReimbursementRequestCommand command,
        CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(command.EmployeeId, cancellationToken)
            ?? throw new ReferenceNotFoundException(
                nameof(command.EmployeeId), "Employee", command.EmployeeId);

        var request = ReimbursementRequest.Create(
            employee.Id,
            command.Amount,
            command.Category,
            command.Description);

        await _reimbursementRequests.AddAsync(request, cancellationToken);
        await _reimbursementRequests.SaveChangesAsync(cancellationToken);

        return ReimbursementRequestResponse.From(request, employee.FullName);
    }
}
