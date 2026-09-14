using ExpenseReimbursement.Domain.Entities;

namespace ExpenseReimbursement.Application.Features.Employees.GetEmployees;

public sealed record EmployeeResponse(Guid Id, string FullName)
{
    public static EmployeeResponse From(Employee employee) => new(employee.Id, employee.FullName);
}
