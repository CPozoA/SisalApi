using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Vehículos
{
    public record ActualizarVehiculoCommand(int Id, string Placa, string Marca, string Modelo, string? Color)
        : ICommand<bool>;

    public sealed class ActualizarVehiculoCommandValidator : AbstractValidator<ActualizarVehiculoCommand>
    {
        public ActualizarVehiculoCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Placa).NotEmpty().WithMessage("La placa es obligatoria.").MaximumLength(10);
            RuleFor(x => x.Marca).NotEmpty().WithMessage("La marca es obligatoria.").MaximumLength(50);
            RuleFor(x => x.Modelo).NotEmpty().WithMessage("El modelo es obligatorio.").MaximumLength(50);
            RuleFor(x => x.Color).MaximumLength(30);
        }
    }

    public sealed class ActualizarVehiculoCommandHandler(IApplicationDbContext db)
        : ICommandHandler<ActualizarVehiculoCommand, bool>
    {
        public async Task<bool> Handle(ActualizarVehiculoCommand command, CancellationToken cancellationToken)
        {
            var vehiculo = await db.Vehiculos
                .FirstOrDefaultAsync(v => v.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el vehículo con id {command.Id}.");

            var placa = command.Placa.Trim().ToUpperInvariant();

            var duplicado = await db.Vehiculos.AnyAsync(
                v => v.Id != command.Id && v.Placa == placa, cancellationToken);
            if (duplicado)
            {
                throw new ValidationException(
                [
                    new ValidationFailure(nameof(command.Placa), "Ya existe otro vehículo con esa placa.")
                ]);
            }

            vehiculo.Placa = placa;
            vehiculo.Marca = command.Marca.Trim();
            vehiculo.Modelo = command.Modelo.Trim();
            vehiculo.Color = command.Color?.Trim();

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
