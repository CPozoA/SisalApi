using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Domain.Entities;
using SiSal.Infrastructure.Identity;
using System.Reflection;

namespace SiSal.Infrastructure.Persistence
{
    public class SisalDbContext(DbContextOptions<SisalDbContext> options) : DbContext(options), IApplicationDbContext
    {

        public DbSet<Oficina> Oficinas => Set<Oficina>();

        public DbSet<Empleado> Empleados => Set<Empleado>();

        public DbSet<AsignacionPrivilegio> Privilegios => Set<AsignacionPrivilegio>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
