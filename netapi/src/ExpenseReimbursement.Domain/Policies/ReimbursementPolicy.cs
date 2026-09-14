namespace ExpenseReimbursement.Domain.Policies;

// DECISIÓN DE DISEÑO: el umbral de aprobación automática es una constante en una única clase
// de política, y la comparación se expresa como un método con nombre de negocio.
// POR QUÉ: la alternativa era comparar contra 500_000 dentro de Create(); eso esparce el número
// por el código y obliga a buscarlo con grep cuando la empresa cambie la política.
// CONSECUENCIA: cambiar el umbral, la moneda o la inclusividad de la regla toca exactamente
// este archivo y ningún otro.
public static class ReimbursementPolicy
{
    public const decimal AutomaticApprovalThresholdCop = 500_000m;

    public const string Currency = "COP";

    public const int MaxDescriptionLength = 500;

    public const int MaxRejectionReasonLength = 500;

    // El enunciado dice "$500.000 COP o menos", por eso la comparación es inclusiva.
    public static bool QualifiesForAutomaticApproval(decimal amount) =>
        amount <= AutomaticApprovalThresholdCop;
}
