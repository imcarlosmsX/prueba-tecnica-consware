namespace ExpenseReimbursement.Application.Common;

/// <summary>El recurso direccionado por la ruta no existe. Se traduce a HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string resourceName, Guid id)
        : base($"{resourceName} with id '{id}' was not found.")
    {
    }
}
