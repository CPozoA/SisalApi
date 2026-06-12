using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Delegaciones.Common;

namespace Sisal.Application.Delegaciones
{
    public record ObtenerDelegacionPorIdQuery(int Id) : IQuery<DelegacionDto>;

    public sealed class ObtenerDelegacionPorIdQueryHandler(IApplicationDbContext db)
        : IQueryHandler<ObtenerDelegacionPorIdQuery, DelegacionDto>
    {
        public async Task<DelegacionDto> Handle(ObtenerDelegacionPorIdQuery query, CancellationToken cancellationToken)
            => await db.Delegaciones.AsNoTracking()
                .Where(d => d.Id == query.Id)
                .Select(DelegacionProjections.ToDto)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la delegación con id {query.Id}.");
    }
}
