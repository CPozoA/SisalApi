using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record RegistrarRetornoCommand(int SolicitudId) : ICommand<bool>;

    public sealed class RegistrarRetornoCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IUsuarioActualPrivilegios privilegios,
        IDateTime clock)
        : ICommandHandler<RegistrarRetornoCommand, bool>
    {
        public async Task<bool> Handle(RegistrarRetornoCommand command, CancellationToken cancellationToken)
        {
            var actorId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            if (!await privilegios.TienePrivilegioExactoAsync(TipoPrivilegio.Vigilancia, cancellationToken))
                throw new UnauthorizedAccessException("Necesitas el privilegio de Vigilancia para esta acción.");

            var solicitud = await db.SolicitudesSalida
                .Include(s => s.TipoPermiso)
                .FirstOrDefaultAsync(s => s.Id == command.SolicitudId, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la solicitud con id {command.SolicitudId}.");

            if (solicitud.Estado != EstadoSolicitud.FueraDeLaInstitucion)
                throw Invalido("La solicitud no está fuera de la institución.");

            var ahora = clock.UtcNow;
            solicitud.HoraRetornoRealUtc = ahora;

            var requiereAnexo = solicitud.TipoPermiso.RequiereAnexoRetorno;
            var nuevoEstado = requiereAnexo
                ? EstadoSolicitud.PendienteAnexoRetorno
                : EstadoSolicitud.Completado;

            var comentario = requiereAnexo
                ? "Retorno registrado. Pendiente de anexo de retorno."
                : "Retorno registrado. Solicitud completada.";

            solicitud.Transicionar(nuevoEstado, actorId, comentario, ahora);

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static ValidationException Invalido(string mensaje)
            => new([new ValidationFailure("Estado", mensaje)]);
    }
}
