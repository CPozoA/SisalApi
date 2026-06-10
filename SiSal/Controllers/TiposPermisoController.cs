using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisal.Application.Common.Messaging;
using Sisal.Application.TiposPermiso;
using Sisal.Application.TiposPermiso.Common;

namespace SiSal.API.Controllers
{
    [ApiController]
    [Route("api/tipos-permiso")]
    [Authorize]
    public sealed class TiposPermisoController(IDispatcher dispatcher) : ControllerBase
    {
        /// <summary>Lista los tipos de permiso activos. El administrador puede incluir los inactivos con incluirInactivos=true.</summary>
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<TipoPermisoDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] bool incluirInactivos, CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerTiposPermisoQuery(incluirInactivos), cancellationToken));

        /// <summary>Obtiene un tipo de permiso por su identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType<TipoPermisoDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerTipoPermisoPorIdQuery(id), cancellationToken));

        /// <summary>Crea un nuevo tipo de permiso. Solo administradores.</summary>
        [HttpPost]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType<TipoPermisoDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Crear(CrearTipoPermisoCommand command, CancellationToken cancellationToken)
        {
            var tipo = await dispatcher.Send(command, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = tipo.Id }, tipo);
        }

        /// <summary>Actualiza un tipo de permiso. Solo administradores.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(int id, ActualizarTipoPermisoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>Activa o desactiva un tipo de permiso. Solo administradores.</summary>
        [HttpPatch("{id:int}/estado")]
        [Authorize(Policy = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoTipoPermisoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
