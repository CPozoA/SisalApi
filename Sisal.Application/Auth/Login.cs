using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using SiSal.Infrastructure.Identity;

namespace Sisal.Application.Auth
{
    public sealed record LoginResultDto(string Token, string RefreshToken, bool DebeCambiarClave);

    public sealed record LoginCommand(string Dni, string Password) : ICommand<LoginResultDto>;

    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Dni).MaximumLength(8).NotEmpty().WithMessage("El DNI es obligatorio.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria.");
        }
    }

    public sealed class LoginCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwt,
    IDateTime clock)
    : ICommandHandler<LoginCommand, LoginResultDto>
    {
        public async Task<LoginResultDto> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var empleado = await db.Empleados
                .FirstOrDefaultAsync(e => e.Dni == command.Dni && e.Activo, cancellationToken);

            if (empleado is null || !passwordHasher.Verify(empleado.PasswordHash, command.Password))
            {
                throw new UnauthorizedAccessException("DNI o contraseña incorrectos.");
            }

            var accessToken = jwt.GenerateAccessToken(empleado);
            var refresh = jwt.GenerateRefreshToken();

            db.RefreshTokens.Add(new RefreshToken
            {
                EmpleadoId = empleado.Id,
                Token = refresh.Token,
                ExpiraEnUtc = refresh.ExpiraEnUtc,
                CreadoEnUtc = clock.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);

            return new LoginResultDto(accessToken, refresh.Token, empleado.DebeCambiarClave);
        }
    }
}
