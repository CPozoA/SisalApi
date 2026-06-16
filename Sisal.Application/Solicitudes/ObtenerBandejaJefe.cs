using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record ObtenerBandejaJefeQuery : IQuery<IReadOnlyList<SolicitudSalidaDto>>;

    public sealed class ObtenerBandejaJefeQueryHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IDateTime clock)
        : IQueryHandler<ObtenerBandejaJefeQuery, IReadOnlyList<SolicitudSalidaDto>>
    {
        public async Task<IReadOnlyList<SolicitudSalidaDto>> Handle(ObtenerBandejaJefeQuery query, CancellationToken cancellationToken)
        {
            var actorId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            var hoy = DateOnly.FromDateTime(clock.NowEnLima.DateTime);

            // Jefes que represento: yo mismo + los titulares que delegaron en mí y están vigentes hoy
            var titulares = await db.Delegaciones.AsNoTracking()
                .Where(d => d.DelegadoId == actorId && d.Activa && d.FechaInicio <= hoy && hoy <= d.FechaFin)
                .Select(d => d.TitularId)
                .ToListAsync(cancellationToken);

            var jefes = titulares.Append(actorId).ToList();

            return await db.SolicitudesSalida.AsNoTracking()
                .Where(s => s.Estado == EstadoSolicitud.PendienteJefe
                         && s.Empleado.JefeInmediatoId != null
                         && jefes.Contains(s.Empleado.JefeInmediatoId.Value))
                .OrderBy(s => s.Fecha).ThenBy(s => s.Id)
                .Select(SolicitudProjections.ToDto)
                .ToListAsync(cancellationToken);
        }
    }
}
