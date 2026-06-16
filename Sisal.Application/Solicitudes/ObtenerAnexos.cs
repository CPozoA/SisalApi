using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record AnexoDto(int Id, TipoAnexo Tipo, string NombreOriginal, string ContentType, long TamanoBytes, DateTime SubidoUtc);

    public record ObtenerAnexosQuery(int SolicitudId) : IQuery<IReadOnlyList<AnexoDto>>;

    public sealed class ObtenerAnexosQueryHandler(IApplicationDbContext db, IVisibilidadSolicitud visibilidad)
        : IQueryHandler<ObtenerAnexosQuery, IReadOnlyList<AnexoDto>>
    {
        public async Task<IReadOnlyList<AnexoDto>> Handle(ObtenerAnexosQuery query, CancellationToken cancellationToken)
        {
            if (!await visibilidad.PuedeVerAsync(query.SolicitudId, cancellationToken))
                throw new UnauthorizedAccessException("No estás autorizado para ver los anexos de esta solicitud.");

            return await db.AnexosArchivo.AsNoTracking()
                .Where(a => a.SolicitudSalidaId == query.SolicitudId)
                .OrderBy(a => a.Tipo)
                .Select(a => new AnexoDto(a.Id, a.Tipo, a.NombreOriginal, a.ContentType, a.TamanoBytes, a.CreatedAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
