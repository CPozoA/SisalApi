using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Oficinas.Commons;

namespace Sisal.Application.Oficinas
{
    public record ObtenerOficinaPorIdQuery(int Id) : IQuery<OficinaDto>;

    public sealed class ObtenerOficinaPorIdQueryHandler(IApplicationDbContext db)
        : IQueryHandler<ObtenerOficinaPorIdQuery, OficinaDto>
    {
        public async Task<OficinaDto> Handle(ObtenerOficinaPorIdQuery query, CancellationToken cancellationToken)
        {
            return await db.Oficinas
                .AsNoTracking()
                .Where(o => o.Id == query.Id)
                .Select(o => new OficinaDto(
                    o.Id, o.Nombre, o.Descripcion, o.Acronimo, o.Activa, o.EsOficinaConductores))
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la oficina con id {query.Id}.");
        }
    }
}
