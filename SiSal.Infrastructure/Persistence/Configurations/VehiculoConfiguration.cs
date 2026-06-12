using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public sealed class VehiculoConfiguration : IEntityTypeConfiguration<Vehiculo>
    {
        public void Configure(EntityTypeBuilder<Vehiculo> builder)
        {
            builder.ToTable("Vehiculos");

            builder.Property(v => v.Placa).IsRequired().HasMaxLength(10);
            builder.HasIndex(v => v.Placa).IsUnique();

            builder.Property(v => v.Marca).IsRequired().HasMaxLength(50);
            builder.Property(v => v.Modelo).IsRequired().HasMaxLength(50);
            builder.Property(v => v.Color).HasMaxLength(30);
        }
    }
}
