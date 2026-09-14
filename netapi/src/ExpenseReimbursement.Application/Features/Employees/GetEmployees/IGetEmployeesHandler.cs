namespace ExpenseReimbursement.Application.Features.Employees.GetEmployees;

public interface IGetEmployeesHandler
{
    Task<IReadOnlyList<EmployeeResponse>> HandleAsync(CancellationToken cancellationToken);
}
