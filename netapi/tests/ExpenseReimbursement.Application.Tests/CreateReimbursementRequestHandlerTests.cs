using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Application.Common;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.CreateReimbursementRequest;
using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Enums;
using ExpenseReimbursement.Domain.Exceptions;
using Moq;

namespace ExpenseReimbursement.Application.Tests;

// DECISIÓN DE DISEÑO: aquí sí se usa Moq, mientras que los tests de Dominio no usan ninguno.
// POR QUÉ: la entidad no tiene colaboradores, así que simular algo sería artificial. El handler
// sí depende de dos repositorios, y lo que hay que verificar es cómo los orquesta: a quién
// consulta, qué delega al dominio y si persiste.
// CONSECUENCIA: los tests fallan por la razón correcta y no necesitan base de datos.
public sealed class CreateReimbursementRequestHandlerTests
{
    private static readonly Guid EmployeeId = Guid.NewGuid();

    private readonly Mock<IReimbursementRequestRepository> _requests = new(MockBehavior.Strict);
    private readonly Mock<IEmployeeRepository> _employees = new(MockBehavior.Strict);
    private readonly CreateReimbursementRequestHandler _handler;

    private ReimbursementRequest? _persistedRequest;

    public CreateReimbursementRequestHandlerTests()
    {
        _handler = new CreateReimbursementRequestHandler(_requests.Object, _employees.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenEmployeeDoesNotExist_ThrowsWithoutTouchingTheRepository()
    {
        GivenEmployeeIsNotFound();

        var command = new CreateReimbursementRequestCommand(
            EmployeeId, 10_000m, ExpenseCategory.Meals, "Gasto");

        var exception = await Assert.ThrowsAsync<ReferenceNotFoundException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        Assert.Equal("EmployeeId", exception.PropertyName);

        // MockBehavior.Strict ya haría fallar cualquier llamada no configurada, pero dejarlo
        // explícito documenta la intención: una solicitud inválida no debe llegar a la base.
        _requests.Verify(
            repository => repository.AddAsync(It.IsAny<ReimbursementRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _requests.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenAmountIsBelowThreshold_PersistsAnApprovedRequest()
    {
        GivenEmployeeExists();
        GivenRepositoryAcceptsWrites();

        var command = new CreateReimbursementRequestCommand(
            EmployeeId, 450_000m, ExpenseCategory.Meals, "Almuerzo con cliente");

        var response = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.Equal(ReimbursementStatus.Approved, response.Status);
        Assert.Equal(ReimbursementStatus.Approved, _persistedRequest!.Status);
        Assert.Equal("Carlos Mendoza", response.EmployeeName);
        Assert.Equal("COP", response.Currency);
    }

    [Fact]
    public async Task HandleAsync_WhenAmountIsAboveThreshold_PersistsAPendingRequest()
    {
        GivenEmployeeExists();
        GivenRepositoryAcceptsWrites();

        var command = new CreateReimbursementRequestCommand(
            EmployeeId, 750_000m, ExpenseCategory.Accommodation, "Hotel en viaje");

        var response = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.Equal(ReimbursementStatus.Pending, response.Status);
        Assert.Equal(ReimbursementStatus.Pending, _persistedRequest!.Status);
    }

    [Fact]
    public async Task HandleAsync_OnSuccess_SavesExactlyOnce()
    {
        GivenEmployeeExists();
        GivenRepositoryAcceptsWrites();

        var command = new CreateReimbursementRequestCommand(
            EmployeeId, 10_000m, ExpenseCategory.Meals, "Gasto");

        await _handler.HandleAsync(command, CancellationToken.None);

        _requests.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // La validación de campos la hace la entidad, no el handler. Este test comprueba que el
    // handler no la intercepta ni la traduce: la deja subir tal cual al middleware.
    [Fact]
    public async Task HandleAsync_WhenAmountIsInvalid_LetsTheDomainExceptionBubbleUp()
    {
        GivenEmployeeExists();

        var command = new CreateReimbursementRequestCommand(
            EmployeeId, -1m, ExpenseCategory.Meals, "Gasto");

        await Assert.ThrowsAsync<DomainValidationException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        _requests.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void GivenEmployeeExists() =>
        _employees
            .Setup(repository => repository.GetByIdAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Employee.Create(EmployeeId, "Carlos Mendoza"));

    private void GivenEmployeeIsNotFound() =>
        _employees
            .Setup(repository => repository.GetByIdAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

    /// <summary>
    /// Permite la escritura y guarda en <see cref="_persistedRequest"/> la entidad que el handler
    /// manda a persistir, para poder afirmar sobre el estado que realmente se guardó y no solo
    /// sobre el que se devolvió al cliente.
    /// </summary>
    private void GivenRepositoryAcceptsWrites()
    {
        _requests
            .Setup(repository => repository.AddAsync(
                It.IsAny<ReimbursementRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ReimbursementRequest, CancellationToken>((request, _) => _persistedRequest = request)
            .Returns(Task.CompletedTask);

        _requests
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
