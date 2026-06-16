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
    public record CrearSolicitudCommand(int TipoPermisoId, string? Motivo, int? VehiculoId)
        : ICommand<SolicitudSalidaDto>;

    public sealed class CrearSolicitudCommandValidator : AbstractValidator<CrearSolicitudCommand>
    {
        public CrearSolicitudCommandValidator()
        {
            RuleFor(x => x.TipoPermisoId).GreaterThan(0).WithMessage("Debe indicar el tipo de permiso.");
            RuleFor(x => x.Motivo).MaximumLength(500);
        }
    }

    public sealed class CrearSolicitudCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IDateTime clock)
        : ICommandHandler<CrearSolicitudCommand, SolicitudSalidaDto>
    {
        public async Task<SolicitudSalidaDto> Handle(CrearSolicitudCommand command, CancellationToken cancellationToken)
        {
            var empleadoId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == empleadoId && e.Activo, cancellationToken)
                ?? throw new UnauthorizedAccessException("El empleado no existe o está inactivo.");

            var tipo = await db.TiposPermiso.FirstOrDefaultAsync(t => t.Id == command.TipoPermisoId && t.Activo, cancellationToken)
                ?? throw Invalido(nameof(command.TipoPermisoId), "El tipo de permiso no existe o está inactivo.");

            Vehiculo? vehiculo = null;
            if (command.VehiculoId is int vehiculoId)
            {
                vehiculo = await db.Vehiculos.FirstOrDefaultAsync(v => v.Id == vehiculoId && v.Activo, cancellationToken)
                    ?? throw Invalido(nameof(command.VehiculoId), "El vehículo no existe o está inactivo.");
            }

            // 1. Una sola solicitud activa por empleado
            var tieneActiva = await db.SolicitudesSalida.AnyAsync(
                s => s.EmpleadoId == empleadoId && EstadosSolicitud.Activos.Contains(s.Estado), cancellationToken);
            if (tieneActiva)
                throw Invalido(nameof(command.TipoPermisoId), "Ya tienes una solicitud en curso. Debes cerrarla antes de pedir otra.");

            // 2. Cuota mensual (mes calendario, hora de Lima)
            var hoy = DateOnly.FromDateTime(clock.NowEnLima.DateTime);
            var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var usadasEsteMes = await db.SolicitudesSalida.CountAsync(
                s => s.EmpleadoId == empleadoId
                  && s.TipoPermisoId == command.TipoPermisoId
                  && s.Fecha >= inicioMes && s.Fecha <= finMes
                  && !EstadosSolicitud.NoCuentanEnCuota.Contains(s.Estado),
                cancellationToken);

            if (usadasEsteMes >= tipo.VecesPorMes)
                throw Invalido(nameof(command.TipoPermisoId),
                    $"Has alcanzado tu cuota mensual ({tipo.VecesPorMes}) para este tipo de permiso.");

            // 3. Estado inicial: si no tiene jefe inmediato (Directora), salta al paso de RRHH
            var estadoInicial = empleado.JefeInmediatoId is null
                ? EstadoSolicitud.PendienteRrhh
                : EstadoSolicitud.PendienteJefe;

            var ahora = clock.UtcNow;

            var solicitud = new SolicitudSalida
            {
                EmpleadoId = empleadoId,
                TipoPermisoId = command.TipoPermisoId,
                VehiculoId = command.VehiculoId,
                Fecha = hoy,
                Motivo = command.Motivo?.Trim(),
                Estado = estadoInicial,
                Historial =
                [
                    new HistorialEstado
                {
                    Estado = estadoInicial,
                    RegistradoPorId = empleadoId,
                    FechaUtc = ahora,
                    Comentario = "Solicitud creada."
                }
                ]
            };

            db.SolicitudesSalida.Add(solicitud);
            await db.SaveChangesAsync(cancellationToken);

            return new SolicitudSalidaDto(
                solicitud.Id,
                empleadoId, $"{empleado.ApellidoPaterno} {empleado.ApellidoMaterno} {empleado.Nombres}",
                tipo.Id, tipo.Nombre,
                solicitud.Fecha, solicitud.Motivo,
                vehiculo?.Id, vehiculo?.Placa,
                solicitud.Estado, solicitud.HoraSalidaRealUtc, solicitud.HoraRetornoRealUtc, solicitud.FueraDePlazo);
        }

        private static ValidationException Invalido(string propiedad, string mensaje)
            => new([new ValidationFailure(propiedad, mensaje)]);
    }
}
