using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;

namespace Sisal.Application.Solicitudes
{
    public record ObtenerMisSolicitudesQuery : IQuery<IReadOnlyList<SolicitudSalidaDto>>;

    public sealed class ObtenerMisSolicitudesQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
        : IQueryHandler<ObtenerMisSolicitudesQuery, IReadOnlyList<SolicitudSalidaDto>>
    {
        public async Task<IReadOnlyList<SolicitudSalidaDto>> Handle(ObtenerMisSolicitudesQuery query, CancellationToken cancellationToken)
        {
            var empleadoId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            return await db.SolicitudesSalida.AsNoTracking()
                .Where(s => s.EmpleadoId == empleadoId)
                .OrderByDescending(s => s.Fecha).ThenByDescending(s => s.Id)
                .Select(SolicitudProjections.ToDto)
                .ToListAsync(cancellationToken);
        }
    }
}
