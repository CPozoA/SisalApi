using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Oficinas.Commons;
using Sisal.Domain.Entities;

namespace Sisal.Application.Oficinas
{
    public record CrearOficinaCommand(string Nombre, string Descripcion, string Acronimo) : ICommand<OficinaDto>;

    public sealed class CrearOficinaCommandValidator : AbstractValidator<CrearOficinaCommand>
    {
        public CrearOficinaCommandValidator()
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(150);
            RuleFor(x => x.Acronimo).NotEmpty().WithMessage("El acrónimo es obligatorio.").MaximumLength(20);
            RuleFor(x => x.Descripcion).MaximumLength(500);
        }
    }

    public sealed class CrearOficinaCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CrearOficinaCommand, OficinaDto>
    {
        public async Task<OficinaDto> Handle(CrearOficinaCommand command, CancellationToken cancellationToken)
        {
            var nombre = command.Nombre.Trim();
            var acronimo = command.Acronimo.Trim();

            var existe = await db.Oficinas.AnyAsync(
                o => o.Nombre == nombre || o.Acronimo == acronimo, cancellationToken);

            if (existe)
            {
                throw new ValidationException(
                [
                    new ValidationFailure(nameof(command.Nombre), "Ya existe una oficina con ese nombre o acrónimo.")
                ]);
            }

            var oficina = new Oficina
            {
                Nombre = nombre,
                Descripcion = command.Descripcion.Trim(),
                Acronimo = acronimo,
                Activa = true,
                EsOficinaConductores = false
            };

            db.Oficinas.Add(oficina);
            await db.SaveChangesAsync(cancellationToken);

            return new OficinaDto(
                oficina.Id, oficina.Nombre, oficina.Descripcion,
                oficina.Acronimo, oficina.Activa, oficina.EsOficinaConductores);

        }
    }
}
