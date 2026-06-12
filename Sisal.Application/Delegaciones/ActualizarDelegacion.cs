using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Delegaciones
{
    public record ActualizarDelegacionCommand(int Id, int DelegadoId, DateOnly FechaInicio, DateOnly FechaFin)
        : ICommand<bool>;

    public sealed class ActualizarDelegacionCommandValidator : AbstractValidator<ActualizarDelegacionCommand>
    {
        public ActualizarDelegacionCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.DelegadoId).GreaterThan(0);
            RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio)
                .WithMessage("La fecha fin no puede ser anterior a la fecha inicio.");
        }
    }

    public sealed class ActualizarDelegacionCommandHandler(IApplicationDbContext db)
        : ICommandHandler<ActualizarDelegacionCommand, bool>
    {
        public async Task<bool> Handle(ActualizarDelegacionCommand command, CancellationToken cancellationToken)
        {
            var delegacion = await db.Delegaciones.FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la delegación con id {command.Id}.");

            if (command.DelegadoId == delegacion.TitularId)
                throw Invalido(nameof(command.DelegadoId), "El delegado debe ser distinto del titular.");

            if (!await db.Empleados.AnyAsync(e => e.Id == command.DelegadoId && e.Activo, cancellationToken))
                throw Invalido(nameof(command.DelegadoId), "El delegado indicado no existe o está inactivo.");

            var solapa = await db.Delegaciones.AnyAsync(
                d => d.Id != command.Id
                  && d.TitularId == delegacion.TitularId
                  && d.Activa
                  && command.FechaInicio <= d.FechaFin
                  && d.FechaInicio <= command.FechaFin,
                cancellationToken);
            if (solapa)
                throw Invalido(nameof(command.FechaInicio), "El titular ya tiene otra delegación activa que se cruza con ese rango.");

            delegacion.DelegadoId = command.DelegadoId;
            delegacion.FechaInicio = command.FechaInicio;
            delegacion.FechaFin = command.FechaFin;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static ValidationException Invalido(string propiedad, string mensaje)
            => new([new ValidationFailure(propiedad, mensaje)]);
    }
}
