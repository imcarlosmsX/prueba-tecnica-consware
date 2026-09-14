using ExpenseReimbursement.Domain.Entities;
using ExpenseReimbursement.Domain.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseReimbursement.Infrastructure.Persistence.Configurations;

public sealed class ReimbursementRequestConfiguration : IEntityTypeConfiguration<ReimbursementRequest>
{
    public void Configure(EntityTypeBuilder<ReimbursementRequest> builder)
    {
        builder.ToTable("ReimbursementRequests");

        builder.HasKey(request => request.Id);

        // Precisión explícita: el dinero nunca se guarda como punto flotante binario, porque
        // 0.1 + 0.2 no es 0.3 en binario y en montos eso termina en descuadres de centavos.
        builder.Property(request => request.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        // Los enums se persisten como texto y no como entero: así la tabla se puede leer
        // directamente con cualquier cliente SQL sin necesitar un diccionario de códigos, y
        // reordenar los miembros del enum no reinterpreta silenciosamente las filas existentes.
        builder.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(request => request.Category)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(request => request.Description)
            .HasMaxLength(ReimbursementPolicy.MaxDescriptionLength)
            .IsRequired();

        builder.Property(request => request.RejectionReason)
            .HasMaxLength(ReimbursementPolicy.MaxRejectionReasonLength);

        builder.Property(request => request.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(request => request.Employee)
            .WithMany()
            .HasForeignKey(request => request.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices sobre exactamente los dos filtros que exige el enunciado.
        builder.HasIndex(request => request.Status);
        builder.HasIndex(request => request.EmployeeId);
    }
}
