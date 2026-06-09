using Sisal.Application.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SiSal.API.Identity
{
    public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
    {
        private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

        public int? EmpleadoId =>
            int.TryParse(Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

        public string? Dni => Principal?.FindFirstValue("dni");

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    }
}
