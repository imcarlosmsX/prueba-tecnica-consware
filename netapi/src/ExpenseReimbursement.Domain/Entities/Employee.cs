using ExpenseReimbursement.Domain.Exceptions;

namespace ExpenseReimbursement.Domain.Entities;

public sealed class Employee
{
    private Employee()
    {
    }

    public Guid Id { get; private set; }

    public string FullName { get; private set; } = null!;

    public static Employee Create(Guid id, string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainValidationException(nameof(FullName), "Employee full name is required.");
        }

        return new Employee
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            FullName = fullName.Trim()
        };
    }
}
