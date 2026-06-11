using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Privilegios.Common;

namespace Sisal.Application.Privilegios
{
    public record ObtenerPrivilegiosDeEmpleadoQuery(int EmpleadoId)
        : IQuery<IReadOnlyList<AsignacionPrivilegioDto>>;

    public sealed class ObtenerPrivilegiosDeEmpleadoQueryHandler(IApplicationDbContext db)
        : IQueryHandler<ObtenerPrivilegiosDeEmpleadoQuery, IReadOnlyList<AsignacionPrivilegioDto>>
    {
        public async Task<IReadOnlyList<AsignacionPrivilegioDto>> Handle(ObtenerPrivilegiosDeEmpleadoQuery query, CancellationToken cancellationToken)
            => await db.Privilegios.AsNoTracking()
                .Where(a => a.EmpleadoId == query.EmpleadoId)
                .OrderBy(a => a.Privilegio)
                .Select(a => new AsignacionPrivilegioDto(
                    a.Id, a.EmpleadoId, a.Privilegio,
                    a.OficinaId, a.Oficina != null ? a.Oficina.Nombre : null, a.Activo))
                .ToListAsync(cancellationToken);
    }
}
