using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Vehículos;
using Sisal.Application.Vehículos.Common;

namespace SiSal.API.Controllers
{
    [ApiController]
    [Route("api/vehiculos")]
    [Authorize]
    public sealed class VehiculosController(IDispatcher dispatcher) : ControllerBase
    {
        /// <summary>Lista los vehículos activos. El administrador puede incluir inactivos con incluirInactivos=true.</summary>
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<VehiculoDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] bool incluirInactivos, CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerVehiculosQuery(incluirInactivos), cancellationToken));

        /// <summary>Obtiene un vehículo por su identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType<VehiculoDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerVehiculoPorIdQuery(id), cancellationToken));

        /// <summary>Crea un vehículo. Solo administradores.</summary>
        [HttpPost]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType<VehiculoDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Crear(CrearVehiculoCommand command, CancellationToken cancellationToken)
        {
            var vehiculo = await dispatcher.Send(command, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = vehiculo.Id }, vehiculo);
        }

        /// <summary>Actualiza un vehículo. Solo administradores.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(int id, ActualizarVehiculoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>Activa o desactiva un vehículo. Solo administradores.</summary>
        [HttpPatch("{id:int}/estado")]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoVehiculoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
