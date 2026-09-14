using ExpenseReimbursement.Domain.Enums;

namespace ExpenseReimbursement.Domain.Exceptions;

/// <summary>
/// La operación es válida en sí misma, pero choca con el estado actual del recurso.
/// Se traduce a HTTP 409 Conflict, no a 400: el cliente no envió nada mal formado.
/// </summary>
public sealed class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(ReimbursementStatus currentStatus, string attemptedAction)
        : base($"A reimbursement request in status '{currentStatus}' cannot be {attemptedAction}. " +
               $"Only requests in status '{ReimbursementStatus.Pending}' can be decided.")
    {
        CurrentStatus = currentStatus;
        AttemptedAction = attemptedAction;
    }

    public ReimbursementStatus CurrentStatus { get; }

    public string AttemptedAction { get; }
}
