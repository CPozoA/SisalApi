using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public sealed class HistorialEstadoConfiguration : IEntityTypeConfiguration<HistorialEstado>
    {
        public void Configure(EntityTypeBuilder<HistorialEstado> builder)
        {
            builder.ToTable("HistorialEstados");
            builder.Property(h => h.Comentario).HasMaxLength(500);

            builder.HasOne<Empleado>().WithMany()
                .HasForeignKey(h => h.RegistradoPorId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.RegistradoPor).WithMany()
                .HasForeignKey(h => h.RegistradoPorId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
