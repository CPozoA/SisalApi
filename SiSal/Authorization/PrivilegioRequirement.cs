using Microsoft.AspNetCore.Authorization;
using Sisal.Domain.Enums;

namespace SiSal.API.Authorization
{
    public sealed class PrivilegioRequirement(TipoPrivilegio privilegio) : IAuthorizationRequirement
    {
        public TipoPrivilegio Privilegio { get; } = privilegio;
    }
}