using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public class OficinaConfiguration : IEntityTypeConfiguration<Oficina>
    {
        public void Configure(EntityTypeBuilder<Oficina> builder)
        {
            builder.ToTable("Oficinas");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Acronimo)
                .HasMaxLength(20)
                .IsRequired();
            
            builder.Property(x => x.Descripcion)
                .HasMaxLength(500);

            builder.HasIndex(x => x.Nombre)
                .IsUnique();

            builder.HasIndex(x => x.Acronimo)
                .IsUnique();

            builder.HasOne(x => x.Jefe)
                .WithMany()
                .HasForeignKey(x => x.JefeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.Empleados)
                .WithOne(x => x.Oficina)
                .HasForeignKey(x => x.OficinaId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
