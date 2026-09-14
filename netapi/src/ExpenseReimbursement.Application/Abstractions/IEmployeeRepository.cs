using ExpenseReimbursement.Domain.Entities;

namespace ExpenseReimbursement.Application.Abstractions;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Employee>> ListAsync(CancellationToken cancellationToken);
}
