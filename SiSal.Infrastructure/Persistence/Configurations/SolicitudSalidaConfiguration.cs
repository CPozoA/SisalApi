using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public sealed class SolicitudSalidaConfiguration : IEntityTypeConfiguration<SolicitudSalida>
    {
        public void Configure(EntityTypeBuilder<SolicitudSalida> builder)
        {
            builder.ToTable("SolicitudesSalida");

            builder.Property(s => s.Motivo).HasMaxLength(500);
            builder.HasIndex(s => new { s.EmpleadoId, s.Estado });   // para hallar rápido la activa

            builder.HasOne(s => s.Empleado).WithMany()
                .HasForeignKey(s => s.EmpleadoId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TipoPermiso).WithMany()
                .HasForeignKey(s => s.TipoPermisoId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Vehiculo).WithMany()
                .HasForeignKey(s => s.VehiculoId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Historial).WithOne(h => h.Solicitud)
                .HasForeignKey(h => h.SolicitudSalidaId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
