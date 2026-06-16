using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Solicitudes
{
    public record ArchivoDescarga(Stream Contenido, string ContentType, string NombreArchivo);

    public record DescargarAnexoQuery(int AnexoId) : IQuery<ArchivoDescarga>;

    public sealed class DescargarAnexoQueryHandler(
        IApplicationDbContext db,
        IAlmacenArchivos almacen,
        IVisibilidadSolicitud visibilidad)
        : IQueryHandler<DescargarAnexoQuery, ArchivoDescarga>
    {
        public async Task<ArchivoDescarga> Handle(DescargarAnexoQuery query, CancellationToken cancellationToken)
        {
            var anexo = await db.AnexosArchivo.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == query.AnexoId, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el anexo con id {query.AnexoId}.");

            if (!await visibilidad.PuedeVerAsync(anexo.SolicitudSalidaId, cancellationToken))
                throw new UnauthorizedAccessException("No estás autorizado para descargar este anexo.");

            var stream = await almacen.AbrirAsync(anexo.RutaArchivo, cancellationToken)
                ?? throw new KeyNotFoundException("El archivo del anexo no se encuentra en el almacenamiento.");

            return new ArchivoDescarga(stream, anexo.ContentType, anexo.NombreOriginal);
        }
    }
}
