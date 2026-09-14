using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;

namespace ExpenseReimbursement.Application.Abstractions;

// DECISIÓN DE DISEÑO: repositorio específico del agregado, con métodos que nombran consultas
// del negocio, en vez de un IGenericRepository<T> o de IQueryable expuesto.
// POR QUÉ: un genérico solo envuelve DbSet y no aporta nada; exponer IQueryable deja que la
// capa de Aplicación escriba SQL implícito y ata los casos de uso al proveedor de datos.
// CONSECUENCIA: cambiar SQL Server por otro motor, o añadir un índice, no toca ningún handler.
public interface IReimbursementRequestRepository
{
    Task<ReimbursementRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ReimbursementRequest>> ListAsync(
        ReimbursementStatus? status,
        Guid? employeeId,
        CancellationToken cancellationToken);

    Task AddAsync(ReimbursementRequest request, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
