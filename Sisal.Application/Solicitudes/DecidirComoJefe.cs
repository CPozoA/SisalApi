using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record DecidirComoJefeCommand(int SolicitudId, bool Aprobar, string? Comentario) : ICommand<bool>;

    public sealed class DecidirComoJefeCommandValidator : AbstractValidator<DecidirComoJefeCommand>
    {
        public DecidirComoJefeCommandValidator()
        {
            RuleFor(x => x.SolicitudId).GreaterThan(0);
            RuleFor(x => x.Comentario).MaximumLength(500);
            RuleFor(x => x.Comentario).NotEmpty().When(x => !x.Aprobar)
                .WithMessage("Debes indicar el motivo del rechazo.");
        }
    }

    public sealed class DecidirComoJefeCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IDateTime clock)
        : ICommandHandler<DecidirComoJefeCommand, bool>
    {
        public async Task<bool> Handle(DecidirComoJefeCommand command, CancellationToken cancellationToken)
        {
            var actorId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            var solicitud = await db.SolicitudesSalida
                .Include(s => s.Empleado)
                .FirstOrDefaultAsync(s => s.Id == command.SolicitudId, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la solicitud con id {command.SolicitudId}.");

            if (solicitud.Estado != EstadoSolicitud.PendienteJefe)
                throw Invalido("La solicitud no está pendiente de aprobación del jefe.");

            var hoy = DateOnly.FromDateTime(clock.NowEnLima.DateTime);
            if (!await PuedeActuarComoJefe(actorId, solicitud.Empleado.JefeInmediatoId, hoy, cancellationToken))
                throw new UnauthorizedAccessException("No estás autorizado para aprobar o rechazar esta solicitud.");

            var nuevoEstado = command.Aprobar
                ? EstadoSolicitud.PendienteRrhh
                : EstadoSolicitud.RechazadoPorJefe;

            solicitud.Transicionar(nuevoEstado, actorId, command.Comentario, clock.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task<bool> PuedeActuarComoJefe(int actorId, int? jefeId, DateOnly hoy, CancellationToken cancellationToken)
        {
            if (jefeId is null) return false;          // sin jefe, este paso no aplica
            if (actorId == jefeId) return true;         // es el jefe titular

            // ¿es delegado activo del jefe, vigente hoy?
            return await db.Delegaciones.AnyAsync(
                d => d.TitularId == jefeId && d.DelegadoId == actorId && d.Activa
                  && d.FechaInicio <= hoy && hoy <= d.FechaFin, cancellationToken);
        }

        private static ValidationException Invalido(string mensaje)
            => new([new ValidationFailure("Estado", mensaje)]);
    }
}
