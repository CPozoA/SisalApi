using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
    {
        public void Configure(EntityTypeBuilder<Empleado> builder)
        {
            builder.ToTable("Empleados");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Dni)
                .HasMaxLength(15)
                .IsRequired();

            builder.HasIndex(x => x.Dni)
                .IsUnique();

            builder.Property(x => x.ApellidoPaterno)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ApellidoMaterno)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Nombres)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Celular)
                .HasMaxLength(20);

            builder.Property(x => x.Correo)
                .HasMaxLength(150);

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.TipoEmpleado)
                .HasConversion<int>();

            builder.HasOne(x => x.JefeInmediato)
                .WithMany()
                .HasForeignKey(x => x.JefeInmediatoId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
