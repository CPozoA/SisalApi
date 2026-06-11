using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Privilegios;
using Sisal.Application.Privilegios.Common;

namespace SiSal.API.Controllers
{
    [ApiController]
    [Route("api/privilegios")]
    [Authorize(Policy = "Administrador")]
    public sealed class PrivilegiosController(IDispatcher dispatcher) : ControllerBase
    {
        /// <summary>Lista los privilegios asignados a un empleado (activos e inactivos).</summary>
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<AsignacionPrivilegioDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] int empleadoId, CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerPrivilegiosDeEmpleadoQuery(empleadoId), cancellationToken));

        /// <summary>Asigna un privilegio a un empleado.</summary>
        [HttpPost]
        [ProducesResponseType<AsignacionPrivilegioDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Asignar(AsignarPrivilegioCommand command, CancellationToken cancellationToken)
        {
            var asignacion = await dispatcher.Send(command, cancellationToken);
            return CreatedAtAction(nameof(Listar), new { empleadoId = asignacion.EmpleadoId }, asignacion);
        }

        /// <summary>Revoca un privilegio por su identificador de asignación.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Revocar(int id, CancellationToken cancellationToken)
        {
            await dispatcher.Send(new RevocarPrivilegioCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
