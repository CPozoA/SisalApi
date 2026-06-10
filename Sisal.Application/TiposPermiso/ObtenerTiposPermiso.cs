using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.TiposPermiso.Common;
using Microsoft.EntityFrameworkCore;

namespace Sisal.Application.TiposPermiso
{
    public record ObtenerTiposPermisoQuery(bool IncluirInactivos = false) : IQuery<IReadOnlyList<TipoPermisoDto>>;

    public sealed class ObtenerTiposPermisoQueryHandler(
        IApplicationDbContext db,
        IUsuarioActualPrivilegios privilegios)
        : IQueryHandler<ObtenerTiposPermisoQuery, IReadOnlyList<TipoPermisoDto>>
    {
        public async Task<IReadOnlyList<TipoPermisoDto>> Handle(ObtenerTiposPermisoQuery query, CancellationToken cancellationToken)
        {
            var consulta = db.TiposPermiso.AsNoTracking();

            var puedeVerInactivos = query.IncluirInactivos
                && await privilegios.EsAdministradorAsync(cancellationToken);

            if (!puedeVerInactivos)
                consulta = consulta.Where(t => t.Activo);

            return await consulta
                .OrderBy(t => t.Nombre)
                .Select(t => new TipoPermisoDto(
                    t.Id, t.Nombre, t.Descripcion, t.VecesPorMes,
                    t.RequiereAnexoSalida, t.RequiereAnexoRetorno, t.Activo))
                .ToListAsync(cancellationToken);
        }
    }
}
