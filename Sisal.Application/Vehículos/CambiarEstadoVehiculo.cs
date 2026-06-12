using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Vehículos
{
    public record CambiarEstadoVehiculoCommand(int Id, bool Activo) : ICommand<bool>;

    public sealed class CambiarEstadoVehiculoCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CambiarEstadoVehiculoCommand, bool>
    {
        public async Task<bool> Handle(CambiarEstadoVehiculoCommand command, CancellationToken cancellationToken)
        {
            var vehiculo = await db.Vehiculos
                .FirstOrDefaultAsync(v => v.Id == command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el vehículo con id {command.Id}.");

            vehiculo.Activo = command.Activo;
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
