using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;

namespace Sisal.Application.Solicitudes
{
    public record ObtenerMiSolicitudActivaQuery : IQuery<SolicitudSalidaDto?>;

    public sealed class ObtenerMiSolicitudActivaQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
        : IQueryHandler<ObtenerMiSolicitudActivaQuery, SolicitudSalidaDto?>
    {
        public async Task<SolicitudSalidaDto?> Handle(ObtenerMiSolicitudActivaQuery query, CancellationToken cancellationToken)
        {
            var empleadoId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            return await db.SolicitudesSalida.AsNoTracking()
                .Where(s => s.EmpleadoId == empleadoId && EstadosSolicitud.Activos.Contains(s.Estado))
                .Select(SolicitudProjections.ToDto)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
