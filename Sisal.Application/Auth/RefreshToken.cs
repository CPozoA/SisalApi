using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using SiSal.Infrastructure.Identity;

namespace Sisal.Application.Auth
{
    public sealed record RefreshTokenResultDto(string Token, string RefreshToken);

    public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResultDto>;

    public sealed class RefreshTokenCommandHandler(
        IApplicationDbContext db,
        IJwtTokenGenerator jwt,
        IDateTime clock)
        : ICommandHandler<RefreshTokenCommand, RefreshTokenResultDto>
    {
        public async Task<RefreshTokenResultDto> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var stored = await db.RefreshTokens
                .Include(t => t.Empleado)
                .FirstOrDefaultAsync(t => t.Token == command.RefreshToken, cancellationToken);

            if (stored is null || stored.RevocadoEnUtc is not null || clock.UtcNow >= stored.ExpiraEnUtc)
            {
                throw new UnauthorizedAccessException("Refresh token inválido o expirado.");
            }

            if (!stored.Empleado.Activo)
            {
                throw new UnauthorizedAccessException("El usuario está inactivo.");
            }

            var nuevoRefresh = jwt.GenerateRefreshToken();
            stored.RevocadoEnUtc = clock.UtcNow;
            stored.ReemplazadoPorToken = nuevoRefresh.Token;

            db.RefreshTokens.Add(new RefreshToken
            {
                EmpleadoId = stored.EmpleadoId,
                Token = nuevoRefresh.Token,
                ExpiraEnUtc = nuevoRefresh.ExpiraEnUtc,
                CreadoEnUtc = clock.UtcNow
            });

            var accessToken = jwt.GenerateAccessToken(stored.Empleado);
            await db.SaveChangesAsync(cancellationToken);

            return new RefreshTokenResultDto(accessToken, nuevoRefresh.Token);
        }
    }

    public sealed record LogoutCommand(string RefreshToken) : ICommand<bool>;

    public sealed class LogoutCommandHandler(IApplicationDbContext db, IDateTime clock)
        : ICommandHandler<LogoutCommand, bool>
    {
        public async Task<bool> Handle(LogoutCommand command, CancellationToken cancellationToken)
        {
            var stored = await db.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == command.RefreshToken && t.RevocadoEnUtc == null, cancellationToken);

            if (stored is not null)
            {
                stored.RevocadoEnUtc = clock.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }
            return true;
        }
    }
}
