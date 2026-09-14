using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Application.Common;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequestById;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequests;
using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;
using Moq;

namespace ExpenseReimbursement.Application.Tests;

public sealed class QueryHandlersTests
{
    private readonly Mock<IReimbursementRequestRepository> _requests = new(MockBehavior.Strict);

    // El filtrado se delega al repositorio para que se traduzca a SQL. Lo que se verifica aquí es
    // que el handler transmita los filtros tal cual, sin filtrar en memoria por su cuenta.
    [Fact]
    public async Task GetAll_PassesBothFiltersToTheRepositoryUnchanged()
    {
        var employeeId = Guid.NewGuid();
        _requests
            .Setup(repository => repository.ListAsync(
                ReimbursementStatus.Pending, employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetReimbursementRequestsHandler(_requests.Object);
        var query = new GetReimbursementRequestsQuery(ReimbursementStatus.Pending, employeeId);

        await handler.HandleAsync(query, CancellationToken.None);

        _requests.Verify(
            repository => repository.ListAsync(
                ReimbursementStatus.Pending, employeeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithoutFilters_PassesNullsSoTheRepositoryReturnsEverything()
    {
        _requests
            .Setup(repository => repository.ListAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetReimbursementRequestsHandler(_requests.Object);

        await handler.HandleAsync(new GetReimbursementRequestsQuery(null, null), CancellationToken.None);

        _requests.Verify(
            repository => repository.ListAsync(null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenThereAreNoResults_ReturnsEmptyListInsteadOfThrowing()
    {
        _requests
            .Setup(repository => repository.ListAsync(
                It.IsAny<ReimbursementStatus?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetReimbursementRequestsHandler(_requests.Object);

        var result = await handler.HandleAsync(
            new GetReimbursementRequestsQuery(null, null), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAll_MapsEveryRequestToAResponse()
    {
        var stored = new[]
        {
            ReimbursementRequest.Create(Guid.NewGuid(), 100_000m, ExpenseCategory.Meals, "Almuerzo"),
            ReimbursementRequest.Create(Guid.NewGuid(), 900_000m, ExpenseCategory.Other, "Imprevisto")
        };

        _requests
            .Setup(repository => repository.ListAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        var handler = new GetReimbursementRequestsHandler(_requests.Object);

        var result = await handler.HandleAsync(
            new GetReimbursementRequestsQuery(null, null), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(ReimbursementStatus.Approved, result[0].Status);
        Assert.Equal(ReimbursementStatus.Pending, result[1].Status);
    }

    [Fact]
    public async Task GetById_WhenRequestDoesNotExist_ThrowsNotFound()
    {
        var missingId = Guid.NewGuid();
        _requests
            .Setup(repository => repository.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReimbursementRequest?)null);

        var handler = new GetReimbursementRequestByIdHandler(_requests.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(missingId, CancellationToken.None));
    }

    [Fact]
    public async Task GetById_WhenRequestExists_ReturnsItsDetail()
    {
        var request = ReimbursementRequest.Create(
            Guid.NewGuid(), 750_000m, ExpenseCategory.Accommodation, "Hotel en viaje");

        _requests
            .Setup(repository => repository.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var handler = new GetReimbursementRequestByIdHandler(_requests.Object);

        var response = await handler.HandleAsync(request.Id, CancellationToken.None);

        Assert.Equal(request.Id, response.Id);
        Assert.Equal(750_000m, response.Amount);
        Assert.Equal(ReimbursementStatus.Pending, response.Status);
        Assert.Equal(ExpenseCategory.Accommodation, response.Category);
    }
}
