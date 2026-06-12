using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public sealed class DelegacionConfiguration : IEntityTypeConfiguration<Delegacion>
    {
        public void Configure(EntityTypeBuilder<Delegacion> builder)
        {
            builder.ToTable("Delegaciones");

            builder.HasOne(d => d.Titular)
                .WithMany()
                .HasForeignKey(d => d.TitularId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Delegado)
                .WithMany()
                .HasForeignKey(d => d.DelegadoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
