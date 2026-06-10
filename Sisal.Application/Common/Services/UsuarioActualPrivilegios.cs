using Sisal.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Sisal.Domain.Enums;

namespace Sisal.Application.Common.Services
{
    public sealed class UsuarioActualPrivilegios(IApplicationDbContext db, ICurrentUser usuario)
        : IUsuarioActualPrivilegios
    {
        public Task<bool> EsAdministradorAsync(CancellationToken cancellationToken = default)
            => TienePrivilegioAsync(TipoPrivilegio.Administrador, cancellationToken);

        public async Task<bool> TienePrivilegioAsync(TipoPrivilegio privilegio, CancellationToken cancellationToken = default)
        {
            var empleadoId = usuario.EmpleadoId;
            if (empleadoId is null) return false;

            // El Administrador pasa cualquier verificación de privilegio.
            return await db.Privilegios.AnyAsync(
                p => p.EmpleadoId == empleadoId && p.Activo &&
                     (p.Privilegio == privilegio || p.Privilegio == TipoPrivilegio.Administrador),
                cancellationToken);
        }
    }
}
