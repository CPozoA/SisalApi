using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Delegaciones.Common;
using Sisal.Domain.Entities;

namespace Sisal.Application.Delegaciones
{
    public record CrearDelegacionCommand(int TitularId, int DelegadoId, DateOnly FechaInicio, DateOnly FechaFin)
        : ICommand<DelegacionDto>;

    public sealed class CrearDelegacionCommandValidator : AbstractValidator<CrearDelegacionCommand>
    {
        public CrearDelegacionCommandValidator()
        {
            RuleFor(x => x.TitularId).GreaterThan(0);
            RuleFor(x => x.DelegadoId).GreaterThan(0)
                .NotEqual(x => x.TitularId).WithMessage("El delegado debe ser distinto del titular.");
            RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio)
                .WithMessage("La fecha fin no puede ser anterior a la fecha inicio.");
        }
    }

    public sealed class CrearDelegacionCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CrearDelegacionCommand, DelegacionDto>
    {
        public async Task<DelegacionDto> Handle(CrearDelegacionCommand command, CancellationToken cancellationToken)
        {
            var titular = await db.Empleados.FirstOrDefaultAsync(e => e.Id == command.TitularId, cancellationToken)
                ?? throw Invalido(nameof(command.TitularId), "El titular indicado no existe.");

            var delegado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == command.DelegadoId && e.Activo, cancellationToken)
                ?? throw Invalido(nameof(command.DelegadoId), "El delegado indicado no existe o está inactivo.");

            // ¿Se cruza con otra delegación activa del mismo titular?
            var solapa = await db.Delegaciones.AnyAsync(
                d => d.TitularId == command.TitularId
                  && d.Activa
                  && command.FechaInicio <= d.FechaFin
                  && d.FechaInicio <= command.FechaFin,
                cancellationToken);
            if (solapa)
                throw Invalido(nameof(command.FechaInicio), "El titular ya tiene una delegación activa que se cruza con ese rango de fechas.");

            var delegacion = new Delegacion
            {
                TitularId = command.TitularId,
                DelegadoId = command.DelegadoId,
                FechaInicio = command.FechaInicio,
                FechaFin = command.FechaFin,
                Activa = true
            };

            db.Delegaciones.Add(delegacion);
            await db.SaveChangesAsync(cancellationToken);

            return new DelegacionDto(
                delegacion.Id,
                delegacion.TitularId, $"{titular.ApellidoPaterno} {titular.ApellidoMaterno} {titular.Nombres}",
                delegacion.DelegadoId, $"{delegado.ApellidoPaterno} {delegado.ApellidoMaterno} {delegado.Nombres}",
                delegacion.FechaInicio, delegacion.FechaFin, delegacion.Activa);
        }

        private static ValidationException Invalido(string propiedad, string mensaje)
            => new([new ValidationFailure(propiedad, mensaje)]);
    }
}
