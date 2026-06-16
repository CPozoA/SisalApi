using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record DecidirComoRrhhCommand(int SolicitudId, bool Aprobar, string? Comentario) : ICommand<bool>;

    public sealed class DecidirComoRrhhCommandValidator : AbstractValidator<DecidirComoRrhhCommand>
    {
        public DecidirComoRrhhCommandValidator()
        {
            RuleFor(x => x.SolicitudId).GreaterThan(0);
            RuleFor(x => x.Comentario).MaximumLength(500);
            RuleFor(x => x.Comentario).NotEmpty().When(x => !x.Aprobar)
                .WithMessage("Debes indicar el motivo del rechazo.");
        }
    }

    public sealed class DecidirComoRrhhCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IUsuarioActualPrivilegios privilegios,
        IDateTime clock)
        : ICommandHandler<DecidirComoRrhhCommand, bool>
    {
        public async Task<bool> Handle(DecidirComoRrhhCommand command, CancellationToken cancellationToken)
        {
            var actorId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            if (!await privilegios.TienePrivilegioExactoAsync(TipoPrivilegio.Rrhh, cancellationToken))
                throw new UnauthorizedAccessException("Necesitas el privilegio de RRHH para esta acción.");

            var solicitud = await db.SolicitudesSalida
                .FirstOrDefaultAsync(s => s.Id == command.SolicitudId, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la solicitud con id {command.SolicitudId}.");

            if (solicitud.Estado != EstadoSolicitud.PendienteRrhh)
                throw Invalido("La solicitud no está pendiente de RRHH.");

            var nuevoEstado = command.Aprobar
                ? EstadoSolicitud.ListoParaSalir
                : EstadoSolicitud.RechazadoPorRrhh;

            solicitud.Transicionar(nuevoEstado, actorId, command.Comentario, clock.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static ValidationException Invalido(string mensaje)
            => new([new ValidationFailure("Estado", mensaje)]);
    }
}
