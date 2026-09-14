namespace ExpenseReimbursement.Application.Common;

// DECISIÓN DE DISEÑO: cuando el cuerpo de un POST apunta a un registro que no existe se lanza
// esta excepción, que se traduce a 400, y no NotFoundException, que se traduce a 404.
// POR QUÉ: en POST /reimbursement-requests la colección sí existe; un 404 le diría al cliente
// que el endpoint está mal, cuando lo que está mal es un campo que él envió.
// CONSECUENCIA: el 404 queda reservado para "la URL apunta a un recurso inexistente".
public sealed class ReferenceNotFoundException : Exception
{
    public ReferenceNotFoundException(string propertyName, string resourceName, Guid id)
        : base($"{resourceName} with id '{id}' does not exist.")
    {
        PropertyName = propertyName;
    }

    public string PropertyName { get; }
}
