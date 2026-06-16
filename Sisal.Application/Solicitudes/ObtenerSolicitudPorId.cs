using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;

namespace Sisal.Application.Solicitudes
{
    public record ObtenerSolicitudPorIdQuery(int Id) : IQuery<SolicitudDetalleDto>;

    public sealed class ObtenerSolicitudPorIdQueryHandler(
        IApplicationDbContext db,
        IVisibilidadSolicitud visibilidad)
        : IQueryHandler<ObtenerSolicitudPorIdQuery, SolicitudDetalleDto>
    {
        public async Task<SolicitudDetalleDto> Handle(ObtenerSolicitudPorIdQuery query, CancellationToken cancellationToken)
        {
            var existe = await db.SolicitudesSalida.AsNoTracking()
                .AnyAsync(s => s.Id == query.Id, cancellationToken);
            if (!existe)
                throw new KeyNotFoundException($"No se encontró la solicitud con id {query.Id}.");

            if (!await visibilidad.PuedeVerAsync(query.Id, cancellationToken))
                throw new UnauthorizedAccessException("No estás autorizado para ver esta solicitud.");

            return await db.SolicitudesSalida.AsNoTracking()
                .Where(s => s.Id == query.Id)
                .Select(s => new SolicitudDetalleDto(
                    new SolicitudSalidaDto(
                        s.Id,
                        s.EmpleadoId, s.Empleado.ApellidoPaterno + " " + s.Empleado.ApellidoMaterno + " " + s.Empleado.Nombres,
                        s.TipoPermisoId, s.TipoPermiso.Nombre,
                        s.Fecha, s.Motivo,
                        s.VehiculoId, s.Vehiculo != null ? s.Vehiculo.Placa : null,
                        s.Estado, s.HoraSalidaRealUtc, s.HoraRetornoRealUtc, s.FueraDePlazo),
                    s.Historial.OrderBy(h => h.FechaUtc).Select(h => new HistorialEntradaDto(
                        h.Estado,
                        h.RegistradoPor != null
                            ? h.RegistradoPor.ApellidoPaterno + " " + h.RegistradoPor.ApellidoMaterno + " " + h.RegistradoPor.Nombres
                            : null,
                        h.FechaUtc, h.Comentario)).ToList()))
                .FirstAsync(cancellationToken);
        }
    }
}
