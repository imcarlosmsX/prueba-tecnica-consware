namespace ExpenseReimbursement.Domain.Exceptions;

// DECISIÓN DE DISEÑO: una raíz común para todo error de negocio, con dos hijas que distinguen
// "los datos están mal" de "el estado no permite esta operación".
// POR QUÉ: la alternativa era devolver un Result/Either, que obliga a propagar y desempaquetar
// el error en cada capa; con excepciones tipadas el camino feliz queda limpio y la traducción
// a HTTP ocurre en un solo middleware.
// CONSECUENCIA: el mapeo excepción -> código HTTP vive en un único lugar y es exhaustivo.
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
