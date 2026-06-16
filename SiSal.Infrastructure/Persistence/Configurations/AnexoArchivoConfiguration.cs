using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public sealed class AnexoArchivoConfiguration : IEntityTypeConfiguration<AnexoArchivo>
    {
        public void Configure(EntityTypeBuilder<AnexoArchivo> builder)
        {
            builder.ToTable("AnexosArchivo");
            builder.Property(a => a.NombreOriginal).IsRequired().HasMaxLength(255);
            builder.Property(a => a.RutaArchivo).IsRequired().HasMaxLength(400);
            builder.Property(a => a.ContentType).IsRequired().HasMaxLength(100);

            builder.HasOne(a => a.Solicitud).WithMany()
                .HasForeignKey(a => a.SolicitudSalidaId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
