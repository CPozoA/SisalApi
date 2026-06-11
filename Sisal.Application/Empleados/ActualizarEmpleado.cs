using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Domain.Enums;

namespace Sisal.Application.Empleados
{
    public record ActualizarEmpleadoCommand(
        int Id,
        string ApellidoPaterno,
        string ApellidoMaterno,
        string Nombres,
        string? Celular,
        string? Correo,
        TipoEmpleado TipoEmpleado,
        int OficinaId,
        int? JefeInmediatoId) : ICommand<bool>;

    public sealed class ActualizarEmpleadoCommandValidator : AbstractValidator<ActualizarEmpleadoCommand>
    {
        public ActualizarEmpleadoCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.ApellidoPaterno).NotEmpty().WithMessage("El apellido paterno es obligatorio.").MaximumLength(100);
            RuleFor(x => x.ApellidoMaterno).NotEmpty().WithMessage("El apellido materno es obligatorio.").MaximumLength(100);
            RuleFor(x => x.Nombres).NotEmpty().WithMessage("Los nombres son obligatorios.").MaximumLength(100);
            RuleFor(x => x.Celular).MaximumLength(20);
            RuleFor(x => x.Correo)
                .EmailAddress().WithMessage("El correo no tiene un formato válido.")
                .When(x => !string.IsNullOrWhiteSpace(x.Correo));
            RuleFor(x => x.TipoEmpleado).IsInEnum().WithMessage("El tipo de empleado no es válido.");
            RuleFor(x => x.OficinaId).GreaterThan(0).WithMessage("Debe indicar una oficina.");
            RuleFor(x => x.JefeInmediatoId)
                .NotNull()
                .When(x => x.TipoEmpleado != TipoEmpleado.F4)
                .WithMessage("Solo la Dirección General (F4) puede no tener jefe inmediato; los demás empleados deben tener uno.");
        }
    }

    public sealed class ActualizarEmpleadoCommandHandler(IApplicationDbContext db)
        : ICommandHandler<ActualizarEmpleadoCommand, bool>
    {
        public async Task<bool> Handle(ActualizarEmpleadoCommand command, CancellationToken cancellationToken)
        {
            var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el empleado con id {command.Id}.");

            if (!await db.Oficinas.AnyAsync(o => o.Id == command.OficinaId, cancellationToken))
                throw Invalido(nameof(command.OficinaId), "La oficina indicada no existe.");

            if (command.JefeInmediatoId is int jefeId)
            {
                if (jefeId == command.Id)
                    throw Invalido(nameof(command.JefeInmediatoId), "Un empleado no puede ser su propio jefe inmediato.");

                if (!await db.Empleados.AnyAsync(e => e.Id == jefeId, cancellationToken))
                    throw Invalido(nameof(command.JefeInmediatoId), "El jefe inmediato indicado no existe.");

                if (await GeneraCiclo(command.Id, jefeId, cancellationToken))
                    throw Invalido(nameof(command.JefeInmediatoId),
                        "Ese jefe generaría un ciclo en la jerarquía (es subordinado, directo o indirecto, de este empleado).");
            }

            empleado.ApellidoPaterno = command.ApellidoPaterno.Trim();
            empleado.ApellidoMaterno = command.ApellidoMaterno.Trim();
            empleado.Nombres = command.Nombres.Trim();
            empleado.Celular = command.Celular?.Trim();
            empleado.Correo = command.Correo?.Trim();
            empleado.TipoEmpleado = command.TipoEmpleado;
            empleado.OficinaId = command.OficinaId;
            empleado.JefeInmediatoId = command.JefeInmediatoId;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static ValidationException Invalido(string propiedad, string mensaje)
        {
            return new([new ValidationFailure(propiedad, mensaje)]);
        }
            

        private async Task<bool> GeneraCiclo(int empleadoId, int jefePropuestoId, CancellationToken cancellationToken)
        {
            var visitados = new HashSet<int>();
            int? actual = jefePropuestoId;

            while (actual is int id)
            {
                if (id == empleadoId) return true;        // el empleado aparece en la cadena → ciclo
                if (!visitados.Add(id)) break;            // datos ya inconsistentes → corta, evita bucle infinito

                actual = await db.Empleados
                    .Where(e => e.Id == id)
                    .Select(e => e.JefeInmediatoId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return false;
        }
    }
}
