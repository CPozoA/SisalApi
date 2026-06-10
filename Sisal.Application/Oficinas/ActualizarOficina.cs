using FluentValidation;
using FluentValidation.Results;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Sisal.Application.Oficinas
{
    public record ActualizarOficinaCommand(int Id, string Nombre, string Descripcion, string Acronimo) : ICommand<bool>;

    public sealed class ActualizarOficinaCommandValidator : AbstractValidator<ActualizarOficinaCommand>
    {
        public ActualizarOficinaCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(150);
            RuleFor(x => x.Acronimo).NotEmpty().WithMessage("El acrónimo es obligatorio.").MaximumLength(20);
            RuleFor(x => x.Descripcion).MaximumLength(500);
        }
    }

    public sealed class ActualizarOficinaCommandHandler(IApplicationDbContext db)
        : ICommandHandler<ActualizarOficinaCommand, bool>
    {
        public async Task<bool> Handle(ActualizarOficinaCommand command, CancellationToken cancellationToken)
        {
            var oficina = await db.Oficinas
                .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la oficina con id {command.Id}.");

            var nombre = command.Nombre.Trim();
            var acronimo = command.Acronimo.Trim();

            // Verifica que el nombre/acrónimo no choque con OTRA oficina
            var duplicado = await db.Oficinas.AnyAsync(
                o => o.Id != command.Id && (o.Nombre == nombre || o.Acronimo == acronimo),
                cancellationToken);

            if (duplicado)
            {
                throw new ValidationException(
                [
                    new ValidationFailure(nameof(command.Nombre), "Ya existe otra oficina con ese nombre o acrónimo.")
                ]);
            }

            oficina.Nombre = nombre;
            oficina.Descripcion = command.Descripcion.Trim();
            oficina.Acronimo = acronimo;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
