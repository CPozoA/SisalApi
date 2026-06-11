using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Empleados
{
    public record CambiarEstadoEmpleadoCommand(int Id, bool Activo) : ICommand<bool>;

    public sealed class CambiarEstadoEmpleadoCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CambiarEstadoEmpleadoCommand, bool>
    {
        public async Task<bool> Handle(CambiarEstadoEmpleadoCommand command, CancellationToken cancellationToken)
        {
            var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el empleado con id {command.Id}.");

            empleado.Activo = command.Activo;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
