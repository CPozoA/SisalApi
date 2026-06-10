using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public sealed class TipoPermisoConfiguration : IEntityTypeConfiguration<TipoPermiso>
    {
        public void Configure(EntityTypeBuilder<TipoPermiso> builder)
        {
            builder.ToTable("TiposPermiso");

            builder.Property(t => t.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(t => t.Nombre)
                .IsUnique();

            builder.Property(t => t.Descripcion)
                .HasMaxLength(500);
        }
    }
}
