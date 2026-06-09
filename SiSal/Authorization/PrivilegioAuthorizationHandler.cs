using Microsoft.AspNetCore.Authorization;
using Sisal.Application.Common.Interfaces;
using Sisal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SiSal.API.Authorization
{
    public sealed class PrivilegioAuthorizationHandler(IApplicationDbContext db, ICurrentUser currentUser)
        : AuthorizationHandler<PrivilegioRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PrivilegioRequirement requirement)
        {
            if (currentUser.EmpleadoId is not int empleadoId)
            {
                return;
            }

            var privilegios = await db.Privilegios
                .Where(p => p.EmpleadoId == empleadoId && p.Activo)
                .Select(p => p.Privilegio)
                .ToListAsync();

            // El Administrador pasa cualquier verificación
            if (privilegios.Contains(TipoPrivilegio.Administrador) || privilegios.Contains(requirement.Privilegio))
            {
                context.Succeed(requirement);
            }
        }
    }
}
