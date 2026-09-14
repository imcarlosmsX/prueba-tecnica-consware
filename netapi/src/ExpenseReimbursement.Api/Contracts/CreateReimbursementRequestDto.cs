using ExpenseReimbursement.Domain.Enums;

namespace ExpenseReimbursement.Api.Contracts;

// DECISIÓN DE DISEÑO: el DTO de entrada es un tipo propio de la capa Api, distinto del Command
// de la capa Application, y no lleva atributos de validación como [Required] o [Range].
// POR QUÉ: si el controller validara el monto con [Range(0.01, ...)], la regla "monto positivo"
// existiría en dos sitios y podrían desincronizarse. La validación de negocio es del Dominio;
// aquí solo se declara la forma del JSON.
// CONSECUENCIA: cambiar el contrato HTTP no obliga a tocar los casos de uso, y al revés.
public sealed record CreateReimbursementRequestDto(
    Guid EmployeeId,
    decimal Amount,
    ExpenseCategory Category,
    string Description);
