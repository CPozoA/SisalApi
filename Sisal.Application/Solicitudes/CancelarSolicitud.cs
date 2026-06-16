using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record CancelarSolicitudCommand(int SolicitudId) : ICommand<bool>;

    public sealed class CancelarSolicitudCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IDateTime clock)
        : ICommandHandler<CancelarSolicitudCommand, bool>
    {
        public async Task<bool> Handle(CancelarSolicitudCommand command, CancellationToken cancellationToken)
        {
            var actorId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            var solicitud = await db.SolicitudesSalida
                .FirstOrDefaultAsync(s => s.Id == command.SolicitudId, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la solicitud con id {command.SolicitudId}.");

            if (solicitud.EmpleadoId != actorId)
                throw new UnauthorizedAccessException("Solo puedes cancelar tus propias solicitudes.");

            if (solicitud.Estado != EstadoSolicitud.PendienteJefe)
                throw new ValidationException(
                [
                    new ValidationFailure("Estado", "Solo puedes cancelar mientras el jefe no haya actuado.")
                ]);

            solicitud.Transicionar(EstadoSolicitud.Cancelado, actorId, "Cancelada por el solicitante.", clock.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
