using Microsoft.EntityFrameworkCore;
using Sisal.Domain.Entities;
using SiSal.Infrastructure.Identity;

namespace Sisal.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Oficina> Oficinas { get; }

        DbSet<Empleado> Empleados { get; }

        DbSet<AsignacionPrivilegio> Privilegios { get; }

        DbSet<RefreshToken> RefreshTokens { get; }

        DbSet<TipoPermiso> TiposPermiso { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
