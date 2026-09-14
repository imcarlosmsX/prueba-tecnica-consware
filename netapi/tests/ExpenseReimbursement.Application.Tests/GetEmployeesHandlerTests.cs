using ExpenseReimbursement.Application.Abstractions;
using ExpenseReimbursement.Application.Features.Employees.GetEmployees;
using ExpenseReimbursement.Domain.Entities;
using Moq;

namespace ExpenseReimbursement.Application.Tests;

public sealed class GetEmployeesHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employees = new(MockBehavior.Strict);

    [Fact]
    public async Task HandleAsync_MapsEveryEmployeeToAResponse()
    {
        var carlosId = Guid.NewGuid();
        _employees
            .Setup(repository => repository.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                Employee.Create(carlosId, "Carlos Mendoza"),
                Employee.Create(Guid.NewGuid(), "Laura Gutiérrez")
            ]);

        var handler = new GetEmployeesHandler(_employees.Object);

        var result = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(carlosId, result[0].Id);
        Assert.Equal("Carlos Mendoza", result[0].FullName);
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoEmployees_ReturnsEmptyList()
    {
        _employees
            .Setup(repository => repository.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetEmployeesHandler(_employees.Object);

        Assert.Empty(await handler.HandleAsync(CancellationToken.None));
    }
}
