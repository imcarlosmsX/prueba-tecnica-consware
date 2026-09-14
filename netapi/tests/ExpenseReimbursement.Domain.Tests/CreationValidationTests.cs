using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;
using ExpenseReimbursement.Domain.Exceptions;
using ExpenseReimbursement.Domain.Policies;

namespace ExpenseReimbursement.Domain.Tests;

/// <summary>Cubre R6: "Valida los datos (monto positivo, categoría y descripción presentes)".</summary>
public sealed class CreationValidationTests
{
    private static readonly Guid EmployeeId = Guid.NewGuid();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500_000)]
    public void Create_WhenAmountIsNotPositive_Throws(decimal amount)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ReimbursementRequest.Create(EmployeeId, amount, ExpenseCategory.Meals, "Gasto"));

        Assert.Equal(nameof(ReimbursementRequest.Amount), exception.PropertyName);
    }

    // Cero es el caso frontera del "monto positivo": no debe colarse como gasto válido ni
    // beneficiarse de la aprobación automática por ser menor que el umbral.
    [Fact]
    public void Create_WhenAmountIsZero_IsRejectedInsteadOfAutoApproved()
    {
        Assert.Throws<DomainValidationException>(() =>
            ReimbursementRequest.Create(EmployeeId, 0m, ExpenseCategory.Meals, "Gasto"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Create_WhenDescriptionIsBlank_Throws(string description)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ReimbursementRequest.Create(EmployeeId, 10_000m, ExpenseCategory.Meals, description));

        Assert.Equal(nameof(ReimbursementRequest.Description), exception.PropertyName);
    }

    [Fact]
    public void Create_WhenDescriptionExceedsMaxLength_Throws()
    {
        var tooLong = new string('a', ReimbursementPolicy.MaxDescriptionLength + 1);

        Assert.Throws<DomainValidationException>(() =>
            ReimbursementRequest.Create(EmployeeId, 10_000m, ExpenseCategory.Meals, tooLong));
    }

    [Fact]
    public void Create_AtExactMaxDescriptionLength_IsAccepted()
    {
        var atLimit = new string('a', ReimbursementPolicy.MaxDescriptionLength);

        var request = ReimbursementRequest.Create(EmployeeId, 10_000m, ExpenseCategory.Meals, atLimit);

        Assert.Equal(atLimit, request.Description);
    }

    [Fact]
    public void Create_WhenCategoryIsNotAValidEnumMember_Throws()
    {
        var invalidCategory = (ExpenseCategory)99;

        var exception = Assert.Throws<DomainValidationException>(() =>
            ReimbursementRequest.Create(EmployeeId, 10_000m, invalidCategory, "Gasto"));

        Assert.Equal(nameof(ReimbursementRequest.Category), exception.PropertyName);
    }

    [Fact]
    public void Create_WhenEmployeeIdIsEmpty_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            ReimbursementRequest.Create(Guid.Empty, 10_000m, ExpenseCategory.Meals, "Gasto"));
    }

    // La columna es decimal(18,2): un tercer decimal se guardaría redondeado en silencio y el
    // monto persistido no coincidiría con el que el empleado escribió.
    [Fact]
    public void Create_WhenAmountHasMoreThanTwoDecimals_Throws()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ReimbursementRequest.Create(EmployeeId, 1000.005m, ExpenseCategory.Meals, "Gasto"));

        Assert.Equal(nameof(ReimbursementRequest.Amount), exception.PropertyName);
    }

    [Fact]
    public void Create_TrimsSurroundingWhitespaceFromDescription()
    {
        var request = ReimbursementRequest.Create(
            EmployeeId, 10_000m, ExpenseCategory.Meals, "   Almuerzo con cliente   ");

        Assert.Equal("Almuerzo con cliente", request.Description);
    }

    [Fact]
    public void Create_AssignsIdentityAndCreationTimestamp()
    {
        var before = DateTime.UtcNow;

        var request = ReimbursementRequest.Create(
            EmployeeId, 10_000m, ExpenseCategory.Meals, "Gasto");

        Assert.NotEqual(Guid.Empty, request.Id);
        Assert.Equal(EmployeeId, request.EmployeeId);
        Assert.InRange(request.CreatedAtUtc, before, DateTime.UtcNow);
    }
}
