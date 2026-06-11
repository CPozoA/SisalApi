using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Empleados.Common;
using Sisal.Domain.Entities;
using Sisal.Domain.Enums;

namespace Sisal.Application.Empleados
{
    public record CrearEmpleadoCommand(
        string Dni,
        string ApellidoPaterno,
        string ApellidoMaterno,
        string Nombres,
        string? Celular,
        string? Correo,
        TipoEmpleado TipoEmpleado,
        int OficinaId,
        int? JefeInmediatoId) : ICommand<EmpleadoDto>;

    public sealed class CrearEmpleadoCommandValidator : AbstractValidator<CrearEmpleadoCommand>
    {
        public CrearEmpleadoCommandValidator()
        {
            RuleFor(x => x.Dni)
                .NotEmpty().WithMessage("El DNI es obligatorio.")
                .Matches(@"^\d{8}$").WithMessage("El DNI debe tener 8 dígitos numéricos.");

            RuleFor(x => x.ApellidoPaterno).NotEmpty().WithMessage("El apellido paterno es obligatorio.").MaximumLength(100);
            RuleFor(x => x.ApellidoMaterno).NotEmpty().WithMessage("El apellido materno es obligatorio.").MaximumLength(100);
            RuleFor(x => x.Nombres).NotEmpty().WithMessage("Los nombres son obligatorios.").MaximumLength(100);

            RuleFor(x => x.Celular).MaximumLength(20);
            RuleFor(x => x.Correo)
                .EmailAddress().WithMessage("El correo no tiene un formato válido.")
                .When(x => !string.IsNullOrWhiteSpace(x.Correo));

            RuleFor(x => x.TipoEmpleado).IsInEnum().WithMessage("El tipo de empleado no es válido.");
            RuleFor(x => x.OficinaId).GreaterThan(0).WithMessage("Debe indicar una oficina.");
        }
    }

    public sealed class CrearEmpleadoCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher)
        : ICommandHandler<CrearEmpleadoCommand, EmpleadoDto>
    {
        public async Task<EmpleadoDto> Handle(CrearEmpleadoCommand command, CancellationToken cancellationToken)
        {
            var dni = command.Dni.Trim();

            if (await db.Empleados.AnyAsync(e => e.Dni == dni, cancellationToken))
            {
                throw Invalido(nameof(command.Dni), "Ya existe un empleado con ese DNI.");
            }
                

            var oficina = await db.Oficinas
                .FirstOrDefaultAsync(o => o.Id == command.OficinaId, cancellationToken)
                ?? throw Invalido(nameof(command.OficinaId), "La oficina indicada no existe.");

            Empleado? jefe = null;

            if (command.JefeInmediatoId is int jefeId)
            {
                jefe = await db.Empleados.FirstOrDefaultAsync(e => e.Id == jefeId, cancellationToken)
                    ?? throw Invalido(nameof(command.JefeInmediatoId), "El jefe inmediato indicado no existe.");
            }

            var empleado = new Empleado
            {
                Dni = dni,
                ApellidoPaterno = command.ApellidoPaterno.Trim(),
                ApellidoMaterno = command.ApellidoMaterno.Trim(),
                Nombres = command.Nombres.Trim(),
                Celular = command.Celular?.Trim(),
                Correo = command.Correo?.Trim(),
                TipoEmpleado = command.TipoEmpleado,
                OficinaId = command.OficinaId,
                JefeInmediatoId = command.JefeInmediatoId,
                PasswordHash = passwordHasher.Hash(AuthDefaults.ClavePorDefecto),
                DebeCambiarClave = true,
                Activo = true
            };

            db.Empleados.Add(empleado);
            await db.SaveChangesAsync(cancellationToken);

            var jefeNombre = jefe is null ? null
                : $"{jefe.ApellidoPaterno} {jefe.ApellidoMaterno} {jefe.Nombres}";

            return new EmpleadoDto(
                empleado.Id, empleado.Dni, empleado.ApellidoPaterno, empleado.ApellidoMaterno,
                empleado.Nombres, empleado.Celular, empleado.Correo, empleado.TipoEmpleado,
                empleado.OficinaId, oficina.Nombre, empleado.JefeInmediatoId, jefeNombre,
                empleado.DebeCambiarClave, empleado.Activo);
        }

        private static ValidationException Invalido(string propiedad, string mensaje)
            => new([new ValidationFailure(propiedad, mensaje)]);
    }
}
