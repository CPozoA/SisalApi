using Sisal.Domain.Entities;

namespace Sisal.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateAccessToken(Empleado empleado);

        RefreshTokenInfo GenerateRefreshToken();
    }

    public sealed record RefreshTokenInfo(string Token, DateTime ExpiraEnUtc);
}
