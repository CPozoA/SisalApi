using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.Auth
{
    public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand<bool>;

    public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
                .NotEqual(x => x.CurrentPassword).WithMessage("La nueva contraseña debe ser distinta de la actual.");
        }
    }

    public sealed class ChangePasswordCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher)
        : ICommandHandler<ChangePasswordCommand, bool>
    {
        public async Task<bool> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var empleadoId = currentUser.EmpleadoId
                ?? throw new UnauthorizedAccessException("No hay un usuario autenticado.");

            var empleado = await db.Empleados
                .FirstOrDefaultAsync(e => e.Id == empleadoId, cancellationToken)
                ?? throw new UnauthorizedAccessException("Usuario no encontrado.");

            if (!passwordHasher.Verify(empleado.PasswordHash, command.CurrentPassword))
            {
                throw new ValidationException(
                [
                    new ValidationFailure(nameof(command.CurrentPassword), "La contraseña actual es incorrecta.")
                ]);
            }

            empleado.PasswordHash = passwordHasher.Hash(command.NewPassword);
            empleado.DebeCambiarClave = false;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
