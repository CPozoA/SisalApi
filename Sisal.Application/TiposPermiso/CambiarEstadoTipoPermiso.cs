using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.TiposPermiso
{
    public record CambiarEstadoTipoPermisoCommand(int Id, bool Activo) : ICommand<bool>;

    public sealed class CambiarEstadoTipoPermisoCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CambiarEstadoTipoPermisoCommand, bool>
    {
        public async Task<bool> Handle(CambiarEstadoTipoPermisoCommand command, CancellationToken cancellationToken)
        {
            var tipo = await db.TiposPermiso
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el tipo de permiso con id {command.Id}.");

            tipo.Activo = command.Activo;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
