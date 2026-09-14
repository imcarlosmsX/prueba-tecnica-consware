using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseReimbursement.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Employee>> ListAsync(CancellationToken cancellationToken) =>
        await _context.Employees
            .AsNoTracking()
            .OrderBy(employee => employee.FullName)
            .ToListAsync(cancellationToken);
}
