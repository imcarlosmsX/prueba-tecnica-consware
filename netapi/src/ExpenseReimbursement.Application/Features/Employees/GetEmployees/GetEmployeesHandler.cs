using ExpenseReimbursement.Application.Abstractions;

namespace ExpenseReimbursement.Application.Features.Employees.GetEmployees;

public sealed class GetEmployeesHandler : IGetEmployeesHandler
{
    private readonly IEmployeeRepository _employees;

    public GetEmployeesHandler(IEmployeeRepository employees)
    {
        _employees = employees;
    }

    public async Task<IReadOnlyList<EmployeeResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var employees = await _employees.ListAsync(cancellationToken);

        return employees.Select(EmployeeResponse.From).ToList();
    }
}
