using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Vehículos.Common;

namespace Sisal.Application.Vehículos
{
    public record ObtenerVehiculoPorIdQuery(int Id) : IQuery<VehiculoDto>;

    public sealed class ObtenerVehiculoPorIdQueryHandler(IApplicationDbContext db)
        : IQueryHandler<ObtenerVehiculoPorIdQuery, VehiculoDto>
    {
        public async Task<VehiculoDto> Handle(ObtenerVehiculoPorIdQuery query, CancellationToken cancellationToken)
            => await db.Vehiculos.AsNoTracking()
                .Where(v => v.Id == query.Id)
                .Select(v => new VehiculoDto(v.Id, v.Placa, v.Marca, v.Modelo, v.Color, v.Activo))
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el vehículo con id {query.Id}.");
    }
}
