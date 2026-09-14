using ExpenseReimbursement.Application.Features.Employees.GetEmployees;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.ApproveReimbursementRequest;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.CreateReimbursementRequest;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequestById;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.GetReimbursementRequests;
using ExpenseReimbursement.Application.Features.ReimbursementRequests.RejectReimbursementRequest;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseReimbursement.Application;

// DECISIÓN DE DISEÑO: cada capa registra sus propias dependencias en un método de extensión,
// y los handlers se listan a mano en vez de descubrirse por reflexión.
// POR QUÉ: seis líneas explícitas fallan en tiempo de compilación si renombro un handler;
// un escaneo de ensamblado falla en tiempo de ejecución y con un mensaje peor.
// CONSECUENCIA: Program.cs no sabe qué casos de uso existen, solo que hay una capa Application.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateReimbursementRequestHandler, CreateReimbursementRequestHandler>();
        services.AddScoped<IGetReimbursementRequestsHandler, GetReimbursementRequestsHandler>();
        services.AddScoped<IGetReimbursementRequestByIdHandler, GetReimbursementRequestByIdHandler>();
        services.AddScoped<IApproveReimbursementRequestHandler, ApproveReimbursementRequestHandler>();
        services.AddScoped<IRejectReimbursementRequestHandler, RejectReimbursementRequestHandler>();
        services.AddScoped<IGetEmployeesHandler, GetEmployeesHandler>();

        return services;
    }
}
