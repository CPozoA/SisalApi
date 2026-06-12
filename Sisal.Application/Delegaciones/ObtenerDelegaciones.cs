using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Delegaciones.Common;

namespace Sisal.Application.Delegaciones
{
    public record ObtenerDelegacionesQuery(int? TitularId = null, bool IncluirInactivas = false)
        : IQuery<IReadOnlyList<DelegacionDto>>;

    public sealed class ObtenerDelegacionesQueryHandler(IApplicationDbContext db)
        : IQueryHandler<ObtenerDelegacionesQuery, IReadOnlyList<DelegacionDto>>
    {
        public async Task<IReadOnlyList<DelegacionDto>> Handle(ObtenerDelegacionesQuery query, CancellationToken cancellationToken)
        {
            var consulta = db.Delegaciones.AsNoTracking();

            if (query.TitularId is int titularId)
                consulta = consulta.Where(d => d.TitularId == titularId);

            if (!query.IncluirInactivas)
                consulta = consulta.Where(d => d.Activa);

            return await consulta
                .OrderByDescending(d => d.FechaInicio)
                .Select(DelegacionProjections.ToDto)
                .ToListAsync(cancellationToken);
        }
    }
}
