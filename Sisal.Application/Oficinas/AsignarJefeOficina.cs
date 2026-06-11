using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Oficinas
{
    public record AsignarJefeOficinaCommand(int OficinaId, int? JefeId) : ICommand<bool>;

    public sealed class AsignarJefeOficinaCommandHandler(IApplicationDbContext db)
        : ICommandHandler<AsignarJefeOficinaCommand, bool>
    {
        public async Task<bool> Handle(AsignarJefeOficinaCommand command, CancellationToken cancellationToken)
        {
            var oficina = await db.Oficinas
                .FirstOrDefaultAsync(o => o.Id == command.OficinaId, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la oficina con id {command.OficinaId}.");

            if (command.JefeId is int jefeId)
            {
                var existeActivo = await db.Empleados.AnyAsync(e => e.Id == jefeId && e.Activo, cancellationToken);
                if (!existeActivo)
                    throw new ValidationException(
                    [
                        new ValidationFailure(nameof(command.JefeId), "El empleado indicado como jefe no existe o está inactivo.")
                    ]);
            }

            oficina.JefeId = command.JefeId;   // null para quitar el jefe
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
