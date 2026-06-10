using FluentValidation;
using FluentValidation.Results;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.TiposPermiso
{
    public record ActualizarTipoPermisoCommand(
        int Id,
        string Nombre,
        string Descripcion,
        int VecesPorMes,
        bool RequiereAnexoSalida,
        bool RequiereAnexoRetorno) : ICommand<bool>;

    public sealed class ActualizarTipoPermisoCommandValidator : AbstractValidator<ActualizarTipoPermisoCommand>
    {
        public ActualizarTipoPermisoCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(150);
            RuleFor(x => x.Descripcion).MaximumLength(500);
            RuleFor(x => x.VecesPorMes).GreaterThan(0).WithMessage("La cuota mensual debe ser al menos 1.");
        }
    }

    public sealed class ActualizarTipoPermisoCommandHandler(IApplicationDbContext db)
        : ICommandHandler<ActualizarTipoPermisoCommand, bool>
    {
        public async Task<bool> Handle(ActualizarTipoPermisoCommand command, CancellationToken cancellationToken)
        {
            var tipo = await db.TiposPermiso
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el tipo de permiso con id {command.Id}.");

            var nombre = command.Nombre.Trim();

            var duplicado = await db.TiposPermiso.AnyAsync(
                t => t.Id != command.Id && t.Nombre == nombre, cancellationToken);
            if (duplicado)
            {
                throw new ValidationException(
                [
                    new ValidationFailure(nameof(command.Nombre), "Ya existe otro tipo de permiso con ese nombre.")
                ]);
            }

            tipo.Nombre = nombre;
            tipo.Descripcion = command.Descripcion.Trim();
            tipo.VecesPorMes = command.VecesPorMes;
            tipo.RequiereAnexoSalida = command.RequiereAnexoSalida;
            tipo.RequiereAnexoRetorno = command.RequiereAnexoRetorno;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
