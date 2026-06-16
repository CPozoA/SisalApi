using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sisal.Application.Solicitudes.Common
{
    public sealed class VisibilidadSolicitud(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IUsuarioActualPrivilegios privilegios,
        IDateTime clock)
        : IVisibilidadSolicitud
    {
        public async Task<bool> PuedeVerAsync(int solicitudId, CancellationToken cancellationToken = default)
        {
            var actorId = currentUser.EmpleadoId;
            if (actorId is null) return false;

            var info = await db.SolicitudesSalida.AsNoTracking()
                .Where(s => s.Id == solicitudId)
                .Select(s => new { s.EmpleadoId, JefeId = s.Empleado.JefeInmediatoId })
                .FirstOrDefaultAsync(cancellationToken);
            if (info is null) return false;

            if (actorId == info.EmpleadoId) return true;
            if (info.JefeId == actorId) return true;
            if (await privilegios.TienePrivilegioExactoAsync(TipoPrivilegio.Rrhh, cancellationToken)) return true;
            if (await privilegios.TienePrivilegioExactoAsync(TipoPrivilegio.Vigilancia, cancellationToken)) return true;
            if (await privilegios.EsAdministradorAsync(cancellationToken)) return true;

            if (info.JefeId is not null)
            {
                var hoy = DateOnly.FromDateTime(clock.NowEnLima.DateTime);
                return await db.Delegaciones.AnyAsync(
                    d => d.TitularId == info.JefeId && d.DelegadoId == actorId.Value && d.Activa
                      && d.FechaInicio <= hoy && hoy <= d.FechaFin, cancellationToken);
            }
            return false;
        }
    }
}
