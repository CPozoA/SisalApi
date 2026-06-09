using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public class AsignacionPrivilegioConfiguration : IEntityTypeConfiguration<AsignacionPrivilegio>
    {
        public void Configure(EntityTypeBuilder<AsignacionPrivilegio> builder)
        {
            builder.ToTable("AsignacionesPrivilegio");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Privilegio).HasConversion<int>();

            builder.HasOne(x => x.Empleado)
                .WithMany(x => x.Privilegios)
                .HasForeignKey(x => x.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Oficina)
                .WithMany()
                .HasForeignKey(x => x.OficinaId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.EmpleadoId, x.Privilegio, x.OficinaId }).IsUnique();
        }
    }
}
