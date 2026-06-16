using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Entities;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record SubirAnexoRetornoCommand(
        int SolicitudId, Stream Contenido, string NombreArchivo, string ContentType, long Tamano)
        : ICommand<bool>;

    public sealed class SubirAnexoRetornoCommandValidator : AbstractValidator<SubirAnexoRetornoCommand>
    {
        private static readonly string[] Permitidos = ["application/pdf", "image/jpeg", "image/png"];

        public SubirAnexoRetornoCommandValidator()
        {
            RuleFor(x => x.SolicitudId).GreaterThan(0);
            RuleFor(x => x.Tamano).GreaterThan(0).LessThanOrEqualTo(10 * 1024 * 1024)
                .WithMessage("El archivo no puede superar los 10 MB.");
            RuleFor(x => x.ContentType).Must(ct => Permitidos.Contains(ct))
                .WithMessage("Solo se permiten archivos PDF, JPG o PNG.");
        }
    }

    public sealed class SubirAnexoRetornoCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IAlmacenArchivos almacen,
        IDateTime clock)
        : ICommandHandler<SubirAnexoRetornoCommand, bool>
    {
        public async Task<bool> Handle(SubirAnexoRetornoCommand command, CancellationToken cancellationToken)
        {
            var actorId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            var solicitud = await db.SolicitudesSalida
                .FirstOrDefaultAsync(s => s.Id == command.SolicitudId, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la solicitud con id {command.SolicitudId}.");

            if (solicitud.EmpleadoId != actorId)
                throw new UnauthorizedAccessException("Solo puedes adjuntar anexos a tus propias solicitudes.");

            if (solicitud.Estado != EstadoSolicitud.PendienteAnexoRetorno)
                throw Invalido("La solicitud no está pendiente de anexo de retorno.");

            var ruta = await almacen.GuardarAsync(command.Contenido, ExtensionDe(command.ContentType), cancellationToken);

            db.AnexosArchivo.Add(new AnexoArchivo
            {
                SolicitudSalidaId = solicitud.Id,
                Tipo = TipoAnexo.Retorno,
                NombreOriginal = command.NombreArchivo,
                RutaArchivo = ruta,
                ContentType = command.ContentType,
                TamanoBytes = command.Tamano
            });

            // Adjuntar el retorno completa la solicitud
            solicitud.Transicionar(EstadoSolicitud.Completado, actorId,
                "Anexo de retorno adjuntado. Solicitud completada.", clock.UtcNow);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await almacen.EliminarAsync(ruta, cancellationToken);
                throw;
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
