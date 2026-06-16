using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record ObtenerBandejaRrhhQuery : IQuery<IReadOnlyList<SolicitudSalidaDto>>;

    public sealed class ObtenerBandejaRrhhQueryHandler(
        IApplicationDbContext db,
        IUsuarioActualPrivilegios privilegios)
        : IQueryHandler<ObtenerBandejaRrhhQuery, IReadOnlyList<SolicitudSalidaDto>>
    {
        public async Task<IReadOnlyList<SolicitudSalidaDto>> Handle(ObtenerBandejaRrhhQuery query, CancellationToken cancellationToken)
        {
            if (!await privilegios.TienePrivilegioExactoAsync(TipoPrivilegio.Rrhh, cancellationToken))
                throw new UnauthorizedAccessException("Necesitas el privilegio de RRHH para ver esta bandeja.");

            return await db.SolicitudesSalida.AsNoTracking()
                .Where(s => s.Estado == EstadoSolicitud.PendienteRrhh)
                .OrderBy(s => s.Fecha).ThenBy(s => s.Id)
                .Select(SolicitudProjections.ToDto)
                .ToListAsync(cancellationToken);
        }
    }
}
