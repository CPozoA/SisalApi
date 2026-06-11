using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Privilegios
{
    public record RevocarPrivilegioCommand(int Id) : ICommand<bool>;

    public sealed class RevocarPrivilegioCommandHandler(IApplicationDbContext db)
        : ICommandHandler<RevocarPrivilegioCommand, bool>
    {
        public async Task<bool> Handle(RevocarPrivilegioCommand command, CancellationToken cancellationToken)
        {
            var asignacion = await db.Privilegios
                .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la asignación de privilegio con id {command.Id}.");

            asignacion.Activo = false;
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
