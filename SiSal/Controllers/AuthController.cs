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

        /// <summary>Inicia sesión con DNI y contraseña. Sólo devuelve el token.</summary>
        [HttpPost("login")]
        [ProducesResponseType<LoginResultDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
        {
            
            var result = await dispatcher.Send(command, cancellationToken);
            return Ok(result);

        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await dispatcher.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken)
        {
            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }

        [Authorize(Policy = "Administrador")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
