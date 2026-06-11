using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Empleados.Common;

namespace Sisal.Application.Empleados
{
    public record ObtenerEmpleadoPorIdQuery(int Id) : IQuery<EmpleadoDto>;

    public sealed class ObtenerEmpleadoPorIdQueryHandler(IApplicationDbContext db)
        : IQueryHandler<ObtenerEmpleadoPorIdQuery, EmpleadoDto>
    {
        public async Task<EmpleadoDto> Handle(ObtenerEmpleadoPorIdQuery query, CancellationToken cancellationToken)
            => await db.Empleados.AsNoTracking()
                .Where(e => e.Id == query.Id)
                .Select(EmpleadoProjections.ToDto)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró el empleado con id {query.Id}.");
    }
}
