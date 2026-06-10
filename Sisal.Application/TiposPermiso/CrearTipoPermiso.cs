using FluentValidation;
using FluentValidation.Results;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.TiposPermiso.Common;
using Microsoft.EntityFrameworkCore;
using Sisal.Domain.Entities;

namespace Sisal.Application.TiposPermiso
{
    public record CrearTipoPermisoCommand(
        string Nombre,
        string Descripcion,
        int VecesPorMes,
        bool RequiereAnexoSalida,
        bool RequiereAnexoRetorno) : ICommand<TipoPermisoDto>;

    public sealed class CrearTipoPermisoCommandValidator : AbstractValidator<CrearTipoPermisoCommand>
    {
        public CrearTipoPermisoCommandValidator()
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(150);
            RuleFor(x => x.Descripcion).MaximumLength(500);
            RuleFor(x => x.VecesPorMes).GreaterThan(0).WithMessage("La cuota mensual debe ser al menos 1.");
        }
    }

    public sealed class CrearTipoPermisoCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CrearTipoPermisoCommand, TipoPermisoDto>
    {
        public async Task<TipoPermisoDto> Handle(CrearTipoPermisoCommand command, CancellationToken cancellationToken)
        {
            var nombre = command.Nombre.Trim();

            var existe = await db.TiposPermiso.AnyAsync(t => t.Nombre == nombre, cancellationToken);
            if (existe)
            {
                throw new ValidationException(
                [
                    new ValidationFailure(nameof(command.Nombre), "Ya existe un tipo de permiso con ese nombre.")
                ]);
            }

            var tipo = new TipoPermiso
            {
                Nombre = nombre,
                Descripcion = command.Descripcion.Trim(),
                VecesPorMes = command.VecesPorMes,
                RequiereAnexoSalida = command.RequiereAnexoSalida,
                RequiereAnexoRetorno = command.RequiereAnexoRetorno,
                Activo = true
            };

            db.TiposPermiso.Add(tipo);
            await db.SaveChangesAsync(cancellationToken);

            return new TipoPermisoDto(
                tipo.Id, tipo.Nombre, tipo.Descripcion, tipo.VecesPorMes,
                tipo.RequiereAnexoSalida, tipo.RequiereAnexoRetorno, tipo.Activo);
        }
    }
}
