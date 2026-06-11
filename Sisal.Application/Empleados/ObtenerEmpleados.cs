using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Empleados.Common;

namespace Sisal.Application.Empleados
{
    public record ObtenerEmpleadosQuery(int? OficinaId = null, bool IncluirInactivos = false)
        : IQuery<IReadOnlyList<EmpleadoDto>>;

    public sealed class ObtenerEmpleadosQueryHandler(
        IApplicationDbContext db,
        IUsuarioActualPrivilegios privilegios)
        : IQueryHandler<ObtenerEmpleadosQuery, IReadOnlyList<EmpleadoDto>>
    {
        public async Task<IReadOnlyList<EmpleadoDto>> Handle(ObtenerEmpleadosQuery query, CancellationToken cancellationToken)
        {
            var consulta = db.Empleados.AsNoTracking();

            if (query.OficinaId is int oficinaId)
                consulta = consulta.Where(e => e.OficinaId == oficinaId);

            var puedeVerInactivos = query.IncluirInactivos
                && await privilegios.EsAdministradorAsync(cancellationToken);
            if (!puedeVerInactivos)
                consulta = consulta.Where(e => e.Activo);

            return await consulta
                .OrderBy(e => e.ApellidoPaterno).ThenBy(e => e.ApellidoMaterno).ThenBy(e => e.Nombres)
                .Select(EmpleadoProjections.ToDto)
                .ToListAsync(cancellationToken);
        }
    }
}
