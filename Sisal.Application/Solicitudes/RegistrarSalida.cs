using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes
{
    public record RegistrarSalidaCommand(int SolicitudId) : ICommand<bool>;

    public sealed class RegistrarSalidaCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IUsuarioActualPrivilegios privilegios,
        IDateTime clock)
        : ICommandHandler<RegistrarSalidaCommand, bool>
    {
        public async Task<bool> Handle(RegistrarSalidaCommand command, CancellationToken cancellationToken)
        {
            var actorId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            if (!await privilegios.TienePrivilegioExactoAsync(TipoPrivilegio.Vigilancia, cancellationToken))
                throw new UnauthorizedAccessException("Necesitas el privilegio de Vigilancia para esta acción.");


            var solicitud = await db.SolicitudesSalida
                 .Include(s => s.TipoPermiso)
                 .FirstOrDefaultAsync(s => s.Id == command.SolicitudId, cancellationToken)
                 ?? throw new KeyNotFoundException($"No se encontró la solicitud con id {command.SolicitudId}.");

            if (solicitud.Estado != EstadoSolicitud.ListoParaSalir)
                throw Invalido("La solicitud no está lista para salir.");

            if (solicitud.TipoPermiso.RequiereAnexoSalida)
            {
                var tieneAnexo = await db.AnexosArchivo.AnyAsync(
                    a => a.SolicitudSalidaId == solicitud.Id && a.Tipo == TipoAnexo.Previo, cancellationToken);
                if (!tieneAnexo)
                    throw Invalido("Falta adjuntar el anexo de salida antes de registrar la salida.");
            }

            var ahora = clock.UtcNow;
            solicitud.HoraSalidaRealUtc = ahora;
            solicitud.Transicionar(EstadoSolicitud.FueraDeLaInstitucion, actorId,
                "Salida registrada por vigilancia.", ahora);

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static ValidationException Invalido(string mensaje)
            => new([new ValidationFailure("Estado", mensaje)]);
    }
}
