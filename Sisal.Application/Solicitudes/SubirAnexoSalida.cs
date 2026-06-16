using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Domain.Entities;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record SubirAnexoSalidaCommand(
        int SolicitudId, Stream Contenido, string NombreArchivo, string ContentType, long Tamano)
        : ICommand<bool>;

    public sealed class SubirAnexoSalidaCommandValidator : AbstractValidator<SubirAnexoSalidaCommand>
    {
        private static readonly string[] Permitidos = ["application/pdf", "image/jpeg", "image/png"];

        public SubirAnexoSalidaCommandValidator()
        {
            RuleFor(x => x.SolicitudId).GreaterThan(0);
            RuleFor(x => x.Tamano).GreaterThan(0).LessThanOrEqualTo(10 * 1024 * 1024)
                .WithMessage("El archivo no puede superar los 10 MB.");
            RuleFor(x => x.ContentType).Must(ct => Permitidos.Contains(ct))
                .WithMessage("Solo se permiten archivos PDF, JPG o PNG.");
        }
    }

    public sealed class SubirAnexoSalidaCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IAlmacenArchivos almacen)
        : ICommandHandler<SubirAnexoSalidaCommand, bool>
    {
        public async Task<bool> Handle(SubirAnexoSalidaCommand command, CancellationToken cancellationToken)
        {
            var actorId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            var solicitud = await db.SolicitudesSalida
                .Include(s => s.TipoPermiso)
                .FirstOrDefaultAsync(s => s.Id == command.SolicitudId, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la solicitud con id {command.SolicitudId}.");

            if (solicitud.EmpleadoId != actorId)
                throw new UnauthorizedAccessException("Solo puedes adjuntar anexos a tus propias solicitudes.");

            if (!solicitud.TipoPermiso.RequiereAnexoSalida)
                throw Invalido("Este permiso no requiere anexo de salida.");

            // El previo se sube antes de salir
            if (solicitud.Estado is not (EstadoSolicitud.PendienteJefe or EstadoSolicitud.PendienteRrhh or EstadoSolicitud.ListoParaSalir))
                throw Invalido("Ya no es posible adjuntar el anexo de salida en este estado.");

            var yaExiste = await db.AnexosArchivo.AnyAsync(
                a => a.SolicitudSalidaId == solicitud.Id && a.Tipo == TipoAnexo.Previo, cancellationToken);
            if (yaExiste)
                throw Invalido("Ya se adjuntó el anexo de salida.");

            var ruta = await almacen.GuardarAsync(command.Contenido, ExtensionDe(command.ContentType), cancellationToken);

            db.AnexosArchivo.Add(new AnexoArchivo
            {
                SolicitudSalidaId = solicitud.Id,
                Tipo = TipoAnexo.Previo,
                NombreOriginal = command.NombreArchivo,
                RutaArchivo = ruta,
                ContentType = command.ContentType,
                TamanoBytes = command.Tamano
            });

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await almacen.EliminarAsync(ruta, cancellationToken);   // limpia el huérfano
                throw;                                                    // deja que el error suba al handler global
            }

            return true;
        }

        private static string ExtensionDe(string contentType) => contentType switch
        {
            "application/pdf" => ".pdf",
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            _ => ".bin"
        };

        private static ValidationException Invalido(string mensaje)
            => new([new ValidationFailure("Anexo", mensaje)]);
    }
}
