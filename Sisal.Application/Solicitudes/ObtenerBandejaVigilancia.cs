using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record ObtenerBandejaVigilanciaQuery : IQuery<IReadOnlyList<SolicitudSalidaDto>>;

    public sealed class ObtenerBandejaVigilanciaQueryHandler(
        IApplicationDbContext db,
        IUsuarioActualPrivilegios privilegios)
        : IQueryHandler<ObtenerBandejaVigilanciaQuery, IReadOnlyList<SolicitudSalidaDto>>
    {
        private static readonly EstadoSolicitud[] EstadosVigilancia =
        [
            EstadoSolicitud.ListoParaSalir,
        EstadoSolicitud.FueraDeLaInstitucion
        ];

        public async Task<IReadOnlyList<SolicitudSalidaDto>> Handle(ObtenerBandejaVigilanciaQuery query, CancellationToken cancellationToken)
        {
            if (!await privilegios.TienePrivilegioExactoAsync(TipoPrivilegio.Vigilancia, cancellationToken))
                throw new UnauthorizedAccessException("Necesitas el privilegio de Vigilancia para ver esta bandeja.");

            return await db.SolicitudesSalida.AsNoTracking()
                .Where(s => EstadosVigilancia.Contains(s.Estado))
                .OrderBy(s => s.Fecha).ThenBy(s => s.Id)
                .Select(SolicitudProjections.ToDto)
                .ToListAsync(cancellationToken);
        }
    }
}
