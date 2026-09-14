using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenseReimbursement.Infrastructure.Persistence.Repositories;

public sealed class ReimbursementRequestRepository : IReimbursementRequestRepository
{
    private readonly AppDbContext _context;

    public ReimbursementRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    // Sin AsNoTracking: esta consulta alimenta Approve() y Reject(), que necesitan que el
    // change tracker detecte la modificación para poder guardarla.
    public Task<ReimbursementRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.ReimbursementRequests
            .Include(request => request.Employee)
            .FirstOrDefaultAsync(request => request.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ReimbursementRequest>> ListAsync(
        ReimbursementStatus? status,
        Guid? employeeId,
        CancellationToken cancellationToken)
    {
        var query = _context.ReimbursementRequests
            .AsNoTracking()
            .Include(request => request.Employee)
            .AsQueryable();

        if (status is not null)
        {
            query = query.Where(request => request.Status == status);
        }

        if (employeeId is not null)
        {
            query = query.Where(request => request.EmployeeId == employeeId);
        }

        // Los filtros se traducen a SQL, no se aplican en memoria: la cláusula WHERE viaja a la
        // base y por eso los índices sobre Status y EmployeeId sirven de algo.
        return await query
            .OrderByDescending(request => request.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ReimbursementRequest request, CancellationToken cancellationToken) =>
        await _context.ReimbursementRequests.AddAsync(request, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
