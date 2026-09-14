using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;
using ExpenseReimbursement.Domain.Exceptions;
using ExpenseReimbursement.Domain.Policies;

namespace ExpenseReimbursement.Domain.Tests;

/// <summary>Cubre R4: "Ninguna solicitud puede rechazarse sin un motivo escrito".</summary>
public sealed class RejectionReasonTests
{
    private static readonly Guid EmployeeId = Guid.NewGuid();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n  \t ")]
    public void Reject_WithoutWrittenReason_Throws(string reason)
    {
        var request = CreatePending();

        var exception = Assert.Throws<DomainValidationException>(() => request.Reject(reason));

        Assert.Equal(nameof(ReimbursementRequest.RejectionReason), exception.PropertyName);
    }

    // Un rechazo inválido no puede dejar la solicitud a medias: debe seguir Pendiente y
    // seguir siendo decidible.
    [Fact]
    public void Reject_WithoutReason_LeavesRequestUntouched()
    {
        var request = CreatePending();

        Assert.Throws<DomainValidationException>(() => request.Reject("  "));

        Assert.Equal(ReimbursementStatus.Pending, request.Status);
        Assert.Null(request.RejectionReason);
        Assert.Null(request.DecidedAtUtc);
    }

    [Fact]
    public void Reject_WhenReasonExceedsMaxLength_Throws()
    {
        var request = CreatePending();
        var tooLong = new string('a', ReimbursementPolicy.MaxRejectionReasonLength + 1);

        Assert.Throws<DomainValidationException>(() => request.Reject(tooLong));
    }

    [Fact]
    public void Reject_TrimsSurroundingWhitespaceFromReason()
    {
        var request = CreatePending();

        request.Reject("   Falta el soporte del gasto   ");

        Assert.Equal("Falta el soporte del gasto", request.RejectionReason);
    }

    // El estado se evalúa antes que el motivo: sobre una solicitud ya decidida el conflicto de
    // estado es el problema dominante (409), no la ausencia de motivo (400).
    [Fact]
    public void Reject_OnDecidedRequestWithBlankReason_ThrowsStatusConflictNotValidation()
    {
        var request = CreatePending();
        request.Approve();

        Assert.Throws<InvalidStatusTransitionException>(() => request.Reject(""));
    }

    private static ReimbursementRequest CreatePending() =>
        ReimbursementRequest.Create(
            EmployeeId, 900_000m, ExpenseCategory.OfficeSupplies, "Compra de sillas");
}
