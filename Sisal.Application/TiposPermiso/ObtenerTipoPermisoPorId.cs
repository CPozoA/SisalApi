using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.TiposPermiso.Common;
using Microsoft.EntityFrameworkCore;

namespace Sisal.Application.TiposPermiso
{
    public record ObtenerTipoPermisoPorIdQuery(int Id) : IQuery<TipoPermisoDto>;

    public sealed class ObtenerTipoPermisoPorIdQueryHandler(IApplicationDbContext db)
        : IQueryHandler<ObtenerTipoPermisoPorIdQuery, TipoPermisoDto>
    {
        public async Task<TipoPermisoDto> Handle(ObtenerTipoPermisoPorIdQuery query, CancellationToken cancellationToken)
        {
            return await db.TiposPermiso
                .AsNoTracking()
                .Where(t => t.Id == query.Id)
                .Select(t => new TipoPermisoDto(
                    t.Id, t.Nombre, t.Descripcion, t.VecesPorMes,
                    t.RequiereAnexoSalida, t.RequiereAnexoRetorno, t.Activo))
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el tipo de permiso con id {query.Id}.");
        }
    }
}
