using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Delegaciones;
using Sisal.Application.Delegaciones.Common;

namespace SiSal.API.Controllers
{
    [ApiController]
    [Route("api/delegaciones")]
    [Authorize(Policy = "Administrador")]
    public sealed class DelegacionesController(IDispatcher dispatcher) : ControllerBase
    {

        /// <summary>Lista las delegaciones. Filtra por titular con titularId; incluye inactivas con incluirInactivas=true.</summary>
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<DelegacionDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] int? titularId, [FromQuery] bool incluirInactivas, CancellationToken cancellationToken)
        {
            return Ok(await dispatcher.Query(new ObtenerDelegacionesQuery(titularId, incluirInactivas), cancellationToken));
        }
            

        /// <summary>Obtiene una delegación por su identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType<DelegacionDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
        {
            return Ok(await dispatcher.Query(new ObtenerDelegacionPorIdQuery(id), cancellationToken));
        }
            


        /// <summary>Crea una delegación.</summary>
        [HttpPost]
        [ProducesResponseType<DelegacionDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Crear(CrearDelegacionCommand command, CancellationToken cancellationToken)
        {
            var delegacion = await dispatcher.Send(command, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = delegacion.Id }, delegacion);
        }


        /// <summary>Actualiza una delegación.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(int id, ActualizarDelegacionCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }


        /// <summary>Activa o desactiva una delegación.</summary>
        [HttpPatch("{id:int}/estado")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoDelegacionCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
