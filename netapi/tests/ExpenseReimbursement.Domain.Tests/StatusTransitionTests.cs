using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;
using ExpenseReimbursement.Domain.Exceptions;

namespace ExpenseReimbursement.Domain.Tests;

/// <summary>
/// Cubre R1 (toda solicitud nace Pendiente), R5 (una solicitud decidida no cambia de estado),
/// R7 (solo se aprueba lo Pendiente) y R8 (solo se rechaza lo Pendiente).
/// </summary>
public sealed class StatusTransitionTests
{
    private static readonly Guid EmployeeId = Guid.NewGuid();

    private const decimal AboveThreshold = 900_000m;

    [Fact]
    public void Create_WhenAmountRequiresApproval_StartsPending()
    {
        var request = CreatePending();

        Assert.Equal(ReimbursementStatus.Pending, request.Status);
    }

    [Fact]
    public void Approve_WhenPending_TransitionsToApprovedAndStampsDecision()
    {
        var request = CreatePending();

        request.Approve();

        Assert.Equal(ReimbursementStatus.Approved, request.Status);
        Assert.NotNull(request.DecidedAtUtc);
        Assert.Null(request.RejectionReason);
    }

    [Fact]
    public void Reject_WhenPending_TransitionsToRejectedAndKeepsReason()
    {
        var request = CreatePending();

        request.Reject("Excede el presupuesto del área");

        Assert.Equal(ReimbursementStatus.Rejected, request.Status);
        Assert.Equal("Excede el presupuesto del área", request.RejectionReason);
        Assert.NotNull(request.DecidedAtUtc);
    }

    // R5 en sus cuatro combinaciones posibles. Es la regla que protege la integridad de una
    // decisión ya tomada, y la que un refactor descuidado rompería sin que nadie lo note.
    [Fact]
    public void Approve_WhenAlreadyApproved_Throws()
    {
        var request = CreatePending();
        request.Approve();

        var exception = Assert.Throws<InvalidStatusTransitionException>(() => request.Approve());

        Assert.Equal(ReimbursementStatus.Approved, exception.CurrentStatus);
    }

    [Fact]
    public void Reject_WhenAlreadyApproved_Throws()
    {
        var request = CreatePending();
        request.Approve();

        Assert.Throws<InvalidStatusTransitionException>(() => request.Reject("Motivo cualquiera"));
    }

    [Fact]
    public void Approve_WhenAlreadyRejected_Throws()
    {
        var request = CreatePending();
        request.Reject("Motivo original");

        Assert.Throws<InvalidStatusTransitionException>(() => request.Approve());
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_Throws()
    {
        var request = CreatePending();
        request.Reject("Motivo original");

        Assert.Throws<InvalidStatusTransitionException>(() => request.Reject("Motivo nuevo"));
    }

    // Una solicitud auto-aprobada por monto bajo es terminal desde que nace: el aprobador no
    // puede revisarla después. Es el caso que une R2 con R5.
    [Fact]
    public void Approve_WhenAutomaticallyApprovedOnCreation_Throws()
    {
        var autoApproved = ReimbursementRequest.Create(
            EmployeeId, 100_000m, ExpenseCategory.Meals, "Almuerzo con cliente");

        Assert.Throws<InvalidStatusTransitionException>(() => autoApproved.Approve());
    }

    [Fact]
    public void Reject_WhenDecided_DoesNotOverwriteTheOriginalDecision()
    {
        var request = CreatePending();
        request.Reject("Motivo original");
        var decidedAt = request.DecidedAtUtc;

        Assert.Throws<InvalidStatusTransitionException>(() => request.Reject("Motivo nuevo"));

        Assert.Equal("Motivo original", request.RejectionReason);
        Assert.Equal(decidedAt, request.DecidedAtUtc);
    }

    private static ReimbursementRequest CreatePending() =>
        ReimbursementRequest.Create(
            EmployeeId, AboveThreshold, ExpenseCategory.Transportation, "Viaje a reunión con cliente");
}
