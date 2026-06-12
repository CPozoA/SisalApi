using FluentValidation;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Vehículos.Common;
using Sisal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Results;

namespace Sisal.Application.Vehículos
{
    public record CrearVehiculoCommand(string Placa, string Marca, string Modelo, string? Color)
        : ICommand<VehiculoDto>;

    public sealed class CrearVehiculoCommandValidator : AbstractValidator<CrearVehiculoCommand>
    {
        public CrearVehiculoCommandValidator()
        {
            RuleFor(x => x.Placa).NotEmpty().WithMessage("La placa es obligatoria.").MaximumLength(10);
            RuleFor(x => x.Marca).NotEmpty().WithMessage("La marca es obligatoria.").MaximumLength(50);
            RuleFor(x => x.Modelo).NotEmpty().WithMessage("El modelo es obligatorio.").MaximumLength(50);
            RuleFor(x => x.Color).MaximumLength(30);
        }
    }

    public sealed class CrearVehiculoCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CrearVehiculoCommand, VehiculoDto>
    {
        public async Task<VehiculoDto> Handle(CrearVehiculoCommand command, CancellationToken cancellationToken)
        {
            var placa = command.Placa.Trim().ToUpperInvariant();

            if (await db.Vehiculos.AnyAsync(v => v.Placa == placa, cancellationToken))
            {
                throw new ValidationException(
                [
                    new ValidationFailure(nameof(command.Placa), "Ya existe un vehículo con esa placa.")
                ]);
            }

            var vehiculo = new Vehiculo
            {
                Placa = placa,
                Marca = command.Marca.Trim(),
                Modelo = command.Modelo.Trim(),
                Color = command.Color?.Trim(),
                Activo = true
            };

            db.Vehiculos.Add(vehiculo);
            await db.SaveChangesAsync(cancellationToken);

            return new VehiculoDto(vehiculo.Id, vehiculo.Placa, vehiculo.Marca, vehiculo.Modelo, vehiculo.Color, vehiculo.Activo);
        }
    }
}
