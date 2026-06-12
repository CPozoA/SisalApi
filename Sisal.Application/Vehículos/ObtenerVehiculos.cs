using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Vehículos.Common;

namespace Sisal.Application.Vehículos
{
    public record ObtenerVehiculosQuery(bool IncluirInactivos = false) : IQuery<IReadOnlyList<VehiculoDto>>;

    public sealed class ObtenerVehiculosQueryHandler(
        IApplicationDbContext db,
        IUsuarioActualPrivilegios privilegios)
        : IQueryHandler<ObtenerVehiculosQuery, IReadOnlyList<VehiculoDto>>
    {
        public async Task<IReadOnlyList<VehiculoDto>> Handle(ObtenerVehiculosQuery query, CancellationToken cancellationToken)
        {
            var consulta = db.Vehiculos.AsNoTracking();

            var puedeVerInactivos = query.IncluirInactivos
                && await privilegios.EsAdministradorAsync(cancellationToken);
            if (!puedeVerInactivos)
                consulta = consulta.Where(v => v.Activo);

            return await consulta
                .OrderBy(v => v.Placa)
                .Select(v => new VehiculoDto(v.Id, v.Placa, v.Marca, v.Modelo, v.Color, v.Activo))
                .ToListAsync(cancellationToken);
        }
    }
}
