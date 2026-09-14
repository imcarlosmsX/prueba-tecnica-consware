using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Application.Common;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.ApproveReimbursementRequest;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.RejectReimbursementRequest;
using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;
using ExpenseReimbursement.Domain.Exceptions;
using Moq;

namespace ExpenseReimbursement.Application.Tests;

/// <summary>
/// Verifica que los handlers de decisión distingan "no existe" (404) de lo que decide el
/// dominio (409 o 400), y que solo guarden cuando la transición fue válida.
/// </summary>
public sealed class DecisionHandlersTests
{
    private readonly Mock<IReimbursementRequestRepository> _requests = new(MockBehavior.Strict);
    private readonly ApproveReimbursementRequestHandler _approveHandler;
    private readonly RejectReimbursementRequestHandler _rejectHandler;

    public DecisionHandlersTests()
    {
        _approveHandler = new ApproveReimbursementRequestHandler(_requests.Object);
        _rejectHandler = new RejectReimbursementRequestHandler(_requests.Object);
    }

    [Fact]
    public async Task Approve_WhenRequestDoesNotExist_ThrowsNotFound()
    {
        var missingId = GivenRequestIsNotFound();

        await Assert.ThrowsAsync<NotFoundException>(
            () => _approveHandler.HandleAsync(missingId, CancellationToken.None));
    }

    [Fact]
    public async Task Reject_WhenRequestDoesNotExist_ThrowsNotFound()
    {
        var missingId = GivenRequestIsNotFound();
        var command = new RejectReimbursementRequestCommand(missingId, "Motivo válido");

        await Assert.ThrowsAsync<NotFoundException>(
            () => _rejectHandler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task Approve_WhenPending_ReturnsApprovedAndSaves()
    {
        var request = GivenPendingRequestExists();

        var response = await _approveHandler.HandleAsync(request.Id, CancellationToken.None);

        Assert.Equal(ReimbursementStatus.Approved, response.Status);
        Assert.NotNull(response.DecidedAtUtc);
        _requests.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reject_WhenPending_ReturnsRejectedWithReasonAndSaves()
    {
        var request = GivenPendingRequestExists();
        var command = new RejectReimbursementRequestCommand(request.Id, "Falta el soporte del gasto");

        var response = await _rejectHandler.HandleAsync(command, CancellationToken.None);

        Assert.Equal(ReimbursementStatus.Rejected, response.Status);
        Assert.Equal("Falta el soporte del gasto", response.RejectionReason);
        _requests.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Una transición inválida no debe llegar a SaveChangesAsync: la excepción del dominio corta
    // el flujo antes, y la fila de la base queda intacta.
    [Fact]
    public async Task Approve_WhenAlreadyDecided_ThrowsAndDoesNotSave()
    {
        var request = GivenPendingRequestExists();
        request.Approve();

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => _approveHandler.HandleAsync(request.Id, CancellationToken.None));

        _requests.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Reject_WithoutReason_ThrowsAndDoesNotSave()
    {
        var request = GivenPendingRequestExists();
        var command = new RejectReimbursementRequestCommand(request.Id, "   ");

        await Assert.ThrowsAsync<DomainValidationException>(
            () => _rejectHandler.HandleAsync(command, CancellationToken.None));

        _requests.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private ReimbursementRequest GivenPendingRequestExists()
    {
        var request = ReimbursementRequest.Create(
            Guid.NewGuid(), 900_000m, ExpenseCategory.Transportation, "Viaje a reunión");

        _requests
            .Setup(repository => repository.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        _requests
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return request;
    }

    private Guid GivenRequestIsNotFound()
    {
        var missingId = Guid.NewGuid();

        _requests
            .Setup(repository => repository.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReimbursementRequest?)null);

        return missingId;
    }
}
