using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Oficinas
{
    public record CambiarEstadoOficinaCommand(int Id, bool Activa) : ICommand<bool>;

    public sealed class CambiarEstadoOficinaCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CambiarEstadoOficinaCommand, bool>
    {
        public async Task<bool> Handle(CambiarEstadoOficinaCommand command, CancellationToken cancellationToken)
        {
            var oficina = await db.Oficinas
                .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la oficina con id {command.Id}.");

            oficina.Activa = command.Activa;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
