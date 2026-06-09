using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiSal.Infrastructure.Identity;

namespace SiSal.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Token).HasMaxLength(200).IsRequired();
            builder.HasIndex(x => x.Token).IsUnique();
            builder.Property(x => x.ReemplazadoPorToken).HasMaxLength(200);

            builder.HasOne(x => x.Empleado)
                .WithMany()
                .HasForeignKey(x => x.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
