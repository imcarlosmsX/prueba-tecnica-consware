using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Exceptions;

namespace ExpenseReimbursement.Domain.Tests;

public sealed class EmployeeTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenFullNameIsBlank_Throws(string fullName)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            Employee.Create(Guid.NewGuid(), fullName));

        Assert.Equal(nameof(Employee.FullName), exception.PropertyName);
    }

    [Fact]
    public void Create_TrimsSurroundingWhitespaceFromFullName()
    {
        var employee = Employee.Create(Guid.NewGuid(), "  Carlos Mendoza  ");

        Assert.Equal("Carlos Mendoza", employee.FullName);
    }

    [Fact]
    public void Create_KeepsTheProvidedIdentifier()
    {
        var id = Guid.NewGuid();

        var employee = Employee.Create(id, "Carlos Mendoza");

        Assert.Equal(id, employee.Id);
    }

    // El seeder necesita ids fijos, pero crear un empleado sin id no debe producir una fila
    // con clave vacía que luego choque con otra igual.
    [Fact]
    public void Create_WhenIdIsEmpty_GeneratesOne()
    {
        var employee = Employee.Create(Guid.Empty, "Carlos Mendoza");

        Assert.NotEqual(Guid.Empty, employee.Id);
    }
}
