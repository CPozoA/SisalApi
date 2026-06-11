using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Oficinas;
using Sisal.Application.Oficinas.Commons;

namespace SiSal.API.Controllers
{
    [ApiController]
    [Route("api/oficinas")]
    [Authorize]
    public sealed class OficinasController(IDispatcher dispatcher) : ControllerBase
    {

        /// <summary>Lista las oficinas activas. El administrador puede incluir las inactivas con incluirInactivas=true.</summary>
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<OficinaDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] bool incluirInactivas, CancellationToken cancellationToken)
        {
            return Ok(await dispatcher.Query(new ObtenerOficinasQuery(incluirInactivas), cancellationToken));
        }
            


        /// <summary>Obtiene una oficina por su identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType<OficinaDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
        {
            return Ok(await dispatcher.Query(new ObtenerOficinaPorIdQuery(id), cancellationToken));
        }


        /// <summary>Crea una nueva oficina. Solo administradores.</summary>
        [HttpPost]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType<OficinaDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Crear(CrearOficinaCommand command, CancellationToken cancellationToken)
        {
            var oficina = await dispatcher.Send(command, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = oficina.Id }, oficina);

        }


        /// <summary>Actualiza los datos de una oficina. Solo administradores.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(int id, ActualizarOficinaCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>Activa o desactiva una oficina. Solo administradores.</summary>
        [HttpPatch("{id:int}/estado")]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoOficinaCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }


        /// <summary>Asigna o quita el jefe (responsable administrativo) de una oficina. Solo administradores.</summary>
        [HttpPut("{id:int}/jefe")]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AsignarJefe(int id, AsignarJefeOficinaCommand command, CancellationToken cancellationToken)
        {
            if (id != command.OficinaId)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
