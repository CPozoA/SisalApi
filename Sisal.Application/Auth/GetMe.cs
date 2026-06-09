using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Auth
{
    public sealed record MeDto(
        int Id,
        string Dni,
        string NombreCompleto,
        string Oficina,
        bool DebeCambiarClave,
        IReadOnlyList<string> Privilegios);

    public sealed record GetMeQuery : IQuery<MeDto>;

    public sealed class GetMeQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
        : IQueryHandler<GetMeQuery, MeDto>
    {
        public async Task<MeDto> Handle(GetMeQuery query, CancellationToken cancellationToken)
        {
            var empleadoId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            var empleado = await db.Empleados
                .Include(e => e.Oficina)
                .FirstOrDefaultAsync(e => e.Id == empleadoId, cancellationToken)
                ?? throw new UnauthorizedAccessException("Usuario no encontrado.");

            var privilegios = await db.Privilegios
                .Where(p => p.EmpleadoId == empleadoId && p.Activo)
                .Select(p => p.Privilegio.ToString())
                .ToListAsync(cancellationToken);

            var nombreCompleto = $"{empleado.Nombres} {empleado.ApellidoPaterno} {empleado.ApellidoMaterno}".Trim();

            return new MeDto(empleado.Id, empleado.Dni, nombreCompleto,
                empleado.Oficina.Nombre, empleado.DebeCambiarClave, privilegios);
        }
    }
}
