using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisal.Application.Auth;
using Sisal.Application.Common.Messaging;

namespace SiSal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IDispatcher dispatcher) : ControllerBase
    {

        /// <summary>Inicia sesión con DNI y contraseña y devuelve los tokens.</summary>
        [HttpPost("login")]
        [ProducesResponseType<LoginResultDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
        {
            
            var result = await dispatcher.Send(command, cancellationToken);
            return Ok(result);

        }

        /// <summary>Genera un nuevo par de tokens a partir de un refresh token válido.</summary>
        [HttpPost("refresh")]
        [ProducesResponseType<RefreshTokenResultDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await dispatcher.Send(command, cancellationToken);
            return Ok(result);
        }


        /// <summary>Cierra la sesión revocando el refresh token indicado.</summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken)
        {
            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }


        /// <summary>Cambia la contraseña del usuario autenticado.</summary>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>Restablece la contraseña de un empleado a la clave por defecto del sistema. Solo administradores.</summary>
        [Authorize(Policy = "Administrador")]
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
