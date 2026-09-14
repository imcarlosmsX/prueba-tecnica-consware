using ExpenseReimbursement.Domain.Enums;
using ExpenseReimbursement.Domain.Exceptions;
using ExpenseReimbursement.Domain.Policies;

namespace ExpenseReimbursement.Domain.Entities;

// DECISIÓN DE DISEÑO: agregado con estado encapsulado. Todas las propiedades tienen setter
// privado, la creación pasa obligatoriamente por Create() y el estado solo cambia dentro de
// Approve() y Reject(), que validan la transición antes de aplicarla.
// POR QUÉ: la alternativa era un POCO con setters públicos y la regla en el handler o el
// controller; entonces cualquier código nuevo podría dejar la entidad en un estado imposible
// (rechazada sin motivo, aprobada dos veces) y la regla habría que recordarla en cada punto.
// CONSECUENCIA: es imposible por construcción violar R4 y R5. Si alguien quiere saltarse la
// regla tiene que modificar esta clase, y eso se ve en el diff.
public sealed class ReimbursementRequest
{
    private ReimbursementRequest()
    {
    }

    public Guid Id { get; private set; }

    public Guid EmployeeId { get; private set; }

    public Employee Employee { get; private set; } = null!;

    public decimal Amount { get; private set; }

    public ExpenseCategory Category { get; private set; }

    public string Description { get; private set; } = null!;

    public ReimbursementStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? DecidedAtUtc { get; private set; }

    /// <summary>
    /// Único camino de creación. Valida los datos (R6) y aplica la política de aprobación
    /// automática (R2 y R3) en el mismo acto de nacer.
    /// </summary>
    public static ReimbursementRequest Create(
        Guid employeeId,
        decimal amount,
        ExpenseCategory category,
        string description)
    {
        if (employeeId == Guid.Empty)
        {
            throw new DomainValidationException(nameof(EmployeeId), "Employee id is required.");
        }

        if (amount <= 0)
        {
            throw new DomainValidationException(nameof(Amount), "Amount must be greater than zero.");
        }

        // La columna es decimal(18,2): sin esta guarda, 500000.004 se guardaría redondeado y el
        // monto persistido no coincidiría con el que el empleado vio en pantalla.
        if (decimal.Round(amount, 2) != amount)
        {
            throw new DomainValidationException(
                nameof(Amount), "Amount cannot have more than two decimal places.");
        }

        if (!Enum.IsDefined(category))
        {
            throw new DomainValidationException(nameof(Category), "Category is not a valid expense category.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException(nameof(Description), "Description is required.");
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length > ReimbursementPolicy.MaxDescriptionLength)
        {
            throw new DomainValidationException(
                nameof(Description),
                $"Description cannot exceed {ReimbursementPolicy.MaxDescriptionLength} characters.");
        }

        var createdAtUtc = DateTime.UtcNow;

        var request = new ReimbursementRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Amount = amount,
            Category = category,
            Description = normalizedDescription,
            Status = ReimbursementStatus.Pending,
            CreatedAtUtc = createdAtUtc
        };

        if (ReimbursementPolicy.QualifiesForAutomaticApproval(amount))
        {
            request.Status = ReimbursementStatus.Approved;
            request.DecidedAtUtc = createdAtUtc;
        }

        return request;
    }

    public void Approve()
    {
        EnsureIsPending("approved");

        Status = ReimbursementStatus.Approved;
        DecidedAtUtc = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        EnsureIsPending("rejected");

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException(
                nameof(RejectionReason), "A written reason is required to reject a reimbursement request.");
        }

        var normalizedReason = reason.Trim();

        if (normalizedReason.Length > ReimbursementPolicy.MaxRejectionReasonLength)
        {
            throw new DomainValidationException(
                nameof(RejectionReason),
                $"Rejection reason cannot exceed {ReimbursementPolicy.MaxRejectionReasonLength} characters.");
        }

        Status = ReimbursementStatus.Rejected;
        RejectionReason = normalizedReason;
        DecidedAtUtc = DateTime.UtcNow;
    }

    // El estado se valida ANTES que el motivo: una solicitud ya decidida se rechaza con 409
    // aunque el motivo venga vacío, porque el conflicto de estado es el problema dominante.
    private void EnsureIsPending(string attemptedAction)
    {
        if (Status != ReimbursementStatus.Pending)
        {
            throw new InvalidStatusTransitionException(Status, attemptedAction);
        }
    }
}
