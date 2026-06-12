using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Delegaciones
{
    public record CambiarEstadoDelegacionCommand(int Id, bool Activa) : ICommand<bool>;

    public sealed class CambiarEstadoDelegacionCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CambiarEstadoDelegacionCommand, bool>
    {
        public async Task<bool> Handle(CambiarEstadoDelegacionCommand command, CancellationToken cancellationToken)
        {
            var delegacion = await db.Delegaciones.FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la delegación con id {command.Id}.");

            delegacion.Activa = command.Activa;
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
