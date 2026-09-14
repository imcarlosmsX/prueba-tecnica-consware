using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;
using ExpenseReimbursement.Domain.Policies;

namespace ExpenseReimbursement.Domain.Tests;

// DECISIÓN DE DISEÑO: estos tests no usan mocks ni base de datos, solo la entidad.
// POR QUÉ: la regla de negocio vive en el Dominio, que no tiene dependencias. Si probarla
// exigiera levantar un contexto de EF o simular un repositorio, sería la señal de que la regla
// se filtró a una capa que no le corresponde.
// CONSECUENCIA: la regla más importante del sistema se verifica en milisegundos.
public sealed class AutomaticApprovalRuleTests
{
    private static readonly Guid EmployeeId = Guid.NewGuid();

    // R2: "Las solicitudes de $500.000 COP o menos se aprueban automáticamente al crearse."
    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(499_999)]
    [InlineData(499_999.99)]
    [InlineData(500_000)]
    public void Create_WhenAmountIsAtOrBelowThreshold_ApprovesAutomatically(decimal amount)
    {
        var request = CreateWithAmount(amount);

        Assert.Equal(ReimbursementStatus.Approved, request.Status);
    }

    // R3: "Las solicitudes de más de $500.000 COP quedan Pendientes."
    [Theory]
    [InlineData(500_000.01)]
    [InlineData(500_001)]
    [InlineData(1_200_000)]
    public void Create_WhenAmountIsAboveThreshold_StaysPending(decimal amount)
    {
        var request = CreateWithAmount(amount);

        Assert.Equal(ReimbursementStatus.Pending, request.Status);
    }

    // El umbral es inclusivo: el enunciado dice "$500.000 COP o menos". Este par de asserts es
    // la frontera exacta y es lo primero que rompería si alguien cambiara <= por <.
    [Fact]
    public void Create_AtExactThreshold_IsApproved_ButOneCentAboveIsPending()
    {
        var atThreshold = CreateWithAmount(ReimbursementPolicy.AutomaticApprovalThresholdCop);
        var justAbove = CreateWithAmount(ReimbursementPolicy.AutomaticApprovalThresholdCop + 0.01m);

        Assert.Equal(ReimbursementStatus.Approved, atThreshold.Status);
        Assert.Equal(ReimbursementStatus.Pending, justAbove.Status);
    }

    [Fact]
    public void Create_WhenApprovedAutomatically_StampsDecisionTimestamp()
    {
        var request = CreateWithAmount(100_000);

        Assert.NotNull(request.DecidedAtUtc);
        Assert.Equal(request.CreatedAtUtc, request.DecidedAtUtc);
    }

    [Fact]
    public void Create_WhenPending_LeavesDecisionTimestampEmpty()
    {
        var request = CreateWithAmount(900_000);

        Assert.Null(request.DecidedAtUtc);
    }

    [Fact]
    public void Create_NeverSetsRejectionReason()
    {
        var request = CreateWithAmount(900_000);

        Assert.Null(request.RejectionReason);
    }

    private static ReimbursementRequest CreateWithAmount(decimal amount) =>
        ReimbursementRequest.Create(EmployeeId, amount, ExpenseCategory.Meals, "Gasto de prueba");
}
