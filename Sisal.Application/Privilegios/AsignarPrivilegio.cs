using FluentValidation;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Privilegios.Common;
using Sisal.Domain.Entities;
using Sisal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Results;

namespace Sisal.Application.Privilegios
{
    public record AsignarPrivilegioCommand(int EmpleadoId, TipoPrivilegio Privilegio, int? OficinaId)
        : ICommand<AsignacionPrivilegioDto>;

    public sealed class AsignarPrivilegioCommandValidator : AbstractValidator<AsignarPrivilegioCommand>
    {
        public AsignarPrivilegioCommandValidator()
        {
            RuleFor(x => x.EmpleadoId).GreaterThan(0);
            RuleFor(x => x.Privilegio).IsInEnum().WithMessage("El privilegio no es válido.");
        }
    }

    public sealed class AsignarPrivilegioCommandHandler(IApplicationDbContext db)
        : ICommandHandler<AsignarPrivilegioCommand, AsignacionPrivilegioDto>
    {
        public async Task<AsignacionPrivilegioDto> Handle(AsignarPrivilegioCommand command, CancellationToken cancellationToken)
        {
            if (!await db.Empleados.AnyAsync(e => e.Id == command.EmpleadoId, cancellationToken))
                throw Invalido(nameof(command.EmpleadoId), "El empleado indicado no existe.");

            Oficina? oficina = null;
            if (command.OficinaId is int oficinaId)
            {
                oficina = await db.Oficinas.FirstOrDefaultAsync(o => o.Id == oficinaId, cancellationToken)
                    ?? throw Invalido(nameof(command.OficinaId), "La oficina indicada no existe.");
            }

            // ¿Ya existe esta misma asignación (empleado + privilegio + oficina)?
            var existente = await db.Privilegios.FirstOrDefaultAsync(
                a => a.EmpleadoId == command.EmpleadoId
                  && a.Privilegio == command.Privilegio
                  && a.OficinaId == command.OficinaId, cancellationToken);

            AsignacionPrivilegio asignacion;
            if (existente is not null)
            {
                if (existente.Activo)
                    throw Invalido(nameof(command.Privilegio), "El empleado ya tiene asignado ese privilegio.");

                existente.Activo = true;  // reactivamos en vez de duplicar
                asignacion = existente;
            }
            else
            {
                asignacion = new AsignacionPrivilegio
                {
                    EmpleadoId = command.EmpleadoId,
                    Privilegio = command.Privilegio,
                    OficinaId = command.OficinaId,
                    Activo = true
                };
                db.Privilegios.Add(asignacion);
            }

            await db.SaveChangesAsync(cancellationToken);

            return new AsignacionPrivilegioDto(
                asignacion.Id, asignacion.EmpleadoId, asignacion.Privilegio,
                asignacion.OficinaId, oficina?.Nombre, asignacion.Activo);
        }

        private static ValidationException Invalido(string propiedad, string mensaje)
            => new([new ValidationFailure(propiedad, mensaje)]);
    }
}
