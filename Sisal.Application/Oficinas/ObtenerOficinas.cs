using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Oficinas.Commons;

namespace Sisal.Application.Oficinas
{
    public record ObtenerOficinasQuery(bool IncluirInactivas = false) : IQuery<IReadOnlyList<OficinaDto>>;

    public sealed class ObtenerOficinasQueryHandler(
        IApplicationDbContext db,
        IUsuarioActualPrivilegios privilegios)
        : IQueryHandler<ObtenerOficinasQuery, IReadOnlyList<OficinaDto>>
    {
        public async Task<IReadOnlyList<OficinaDto>> Handle(ObtenerOficinasQuery query, CancellationToken cancellationToken)
        {
            var consulta = db.Oficinas.AsNoTracking();

            // Solo un admin que lo pida explícitamente puede ver las inactivas.
            var puedeVerInactivas = query.IncluirInactivas
                && await privilegios.EsAdministradorAsync(cancellationToken);

            if (!puedeVerInactivas)
                consulta = consulta.Where(o => o.Activa);

            return await consulta
                .OrderBy(o => o.Nombre)
                .Select(o => new OficinaDto(
                    o.Id, o.Nombre, o.Descripcion, o.Acronimo, o.Activa, o.EsOficinaConductores))
                .ToListAsync(cancellationToken);
        }
    }
}
