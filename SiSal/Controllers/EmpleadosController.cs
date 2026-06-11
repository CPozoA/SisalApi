using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Empleados;
using Sisal.Application.Empleados.Common;

namespace SiSal.API.Controllers
{
    [ApiController]
    [Route("api/empleados")]
    [Authorize(Policy = "Administrador")]
    public sealed class EmpleadosController(IDispatcher dispatcher) : ControllerBase
    {
        /// <summary>Lista los empleados. Filtra por oficina con oficinaId; incluye inactivos con incluirInactivos=true.</summary>
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<EmpleadoDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] int? oficinaId, [FromQuery] bool incluirInactivos, CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerEmpleadosQuery(oficinaId, incluirInactivos), cancellationToken));

        /// <summary>Obtiene un empleado por su identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType<EmpleadoDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerEmpleadoPorIdQuery(id), cancellationToken));

        /// <summary>Crea un empleado con la clave por defecto del sistema.</summary>
        [HttpPost]
        [ProducesResponseType<EmpleadoDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Crear(CrearEmpleadoCommand command, CancellationToken cancellationToken)
        {
            var empleado = await dispatcher.Send(command, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = empleado.Id }, empleado);
        }

        /// <summary>Actualiza los datos de un empleado.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(int id, ActualizarEmpleadoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>Activa o desactiva un empleado.</summary>
        [HttpPatch("{id:int}/estado")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoEmpleadoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
