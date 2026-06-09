using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Auth
{
    public sealed record ResetPasswordCommand(int EmpleadoId) : ICommand<bool>;

    public sealed class ResetPasswordCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IDateTime clock)
        : ICommandHandler<ResetPasswordCommand, bool>
    {
        public async Task<bool> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var empleado = await db.Empleados
                .FirstOrDefaultAsync(e => e.Id == command.EmpleadoId, cancellationToken)
                ?? throw new KeyNotFoundException("Empleado no encontrado.");

            empleado.PasswordHash = passwordHasher.Hash(AuthDefaults.ClavePorDefecto);
            empleado.DebeCambiarClave = true;

            // Forzar re-login: revoca los refresh tokens activos del usuario
            var tokensActivos = await db.RefreshTokens
                .Where(t => t.EmpleadoId == command.EmpleadoId && t.RevocadoEnUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var token in tokensActivos)
            {
                token.RevocadoEnUtc = clock.UtcNow;
            }

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
