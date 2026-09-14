namespace ExpenseReimbursement.Domain.Exceptions;

/// <summary>Los datos recibidos no cumplen una invariante del dominio. Se traduce a HTTP 400.</summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string propertyName, string message) : base(message)
    {
        PropertyName = propertyName;
    }

    public string PropertyName { get; }
}
