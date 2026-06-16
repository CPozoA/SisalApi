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

        DbSet<Vehiculo> Vehiculos { get; }

        DbSet<Delegacion> Delegaciones { get; }

        DbSet<SolicitudSalida> SolicitudesSalida { get; }

        DbSet<HistorialEstado> HistorialEstados { get; }

        DbSet<AnexoArchivo> AnexosArchivo { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
