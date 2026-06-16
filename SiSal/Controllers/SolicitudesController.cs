using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Solicitudes;
using Sisal.Application.Solicitudes.Common;

namespace SiSal.API.Controllers
{
    [ApiController]
    [Route("api/solicitudes")]
    [Authorize]
    public sealed class SolicitudesController(IDispatcher dispatcher) : ControllerBase
    {

        /// <summary>Crea una solicitud de salida para el usuario autenticado (para hoy).</summary>
        [HttpPost]
        [ProducesResponseType<SolicitudSalidaDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Crear(CrearSolicitudCommand command, CancellationToken cancellationToken)
        {
            var solicitud = await dispatcher.Send(command, cancellationToken);
            return Created($"/api/solicitudes/{solicitud.Id}", solicitud);
        }


        /// <summary>Decisión del jefe inmediato (o su delegado): aprobar o rechazar.</summary>
        [HttpPost("{id:int}/jefe")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DecidirJefe(int id, DecidirComoJefeCommand command, CancellationToken cancellationToken)
        {
            if (id != command.SolicitudId)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }


        /// <summary>Decisión de RRHH: aprobar o rechazar.</summary>
        [HttpPost("{id:int}/rrhh")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DecidirRrhh(int id, DecidirComoRrhhCommand command, CancellationToken cancellationToken)
        {
            if (id != command.SolicitudId)
                return BadRequest("El id de la ruta no coincide con el del cuerpo.");

            await dispatcher.Send(command, cancellationToken);
            return NoContent();
        }
        

        /// <summary>Vigilancia registra la salida real (de Listo para salir a Fuera de la institución).</summary>
        [HttpPost("{id:int}/salida")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarSalida(int id, CancellationToken cancellationToken)
        {
            await dispatcher.Send(new RegistrarSalidaCommand(id), cancellationToken);
            return NoContent();
        }


        /// <summary>Vigilancia registra el retorno real.</summary>
        [HttpPost("{id:int}/retorno")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarRetorno(int id, CancellationToken cancellationToken)
        {
            await dispatcher.Send(new RegistrarRetornoCommand(id), cancellationToken);
            return NoContent();
        }


        /// <summary>Devuelve la solicitud activa del usuario, o vacío si no tiene ninguna en curso.</summary>
        [HttpGet("mi-activa")]
        [ProducesResponseType<SolicitudSalidaDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> MiActiva(CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerMiSolicitudActivaQuery(), cancellationToken));


        /// <summary>Lista el historial de solicitudes del usuario.</summary>
        [HttpGet("mias")]
        [ProducesResponseType<IReadOnlyList<SolicitudSalidaDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Mias(CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerMisSolicitudesQuery(), cancellationToken));


        /// <summary>Obtiene una solicitud con su línea de tiempo (historial de estados).</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType<SolicitudDetalleDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerSolicitudPorIdQuery(id), cancellationToken));


        /// <summary>Cancela la propia solicitud (solo mientras está pendiente del jefe).</summary>
        [HttpPost("{id:int}/cancelar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
        {
            await dispatcher.Send(new CancelarSolicitudCommand(id), cancellationToken);
            return NoContent();
        }


        /// <summary>Bandeja del jefe: solicitudes pendientes de su aprobación (incluye las recibidas por delegación).</summary>
        [HttpGet("bandeja/jefe")]
        [ProducesResponseType<IReadOnlyList<SolicitudSalidaDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> BandejaJefe(CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerBandejaJefeQuery(), cancellationToken));


        /// <summary>Bandeja de RRHH: solicitudes pendientes de RRHH. Requiere el privilegio de RRHH.</summary>
        [HttpGet("bandeja/rrhh")]
        [ProducesResponseType<IReadOnlyList<SolicitudSalidaDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BandejaRrhh(CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerBandejaRrhhQuery(), cancellationToken));


        /// <summary>Bandeja de vigilancia: solicitudes por salir o fuera de la institución. Requiere el privilegio de Vigilancia.</summary>
        [HttpGet("bandeja/vigilancia")]
        [ProducesResponseType<IReadOnlyList<SolicitudSalidaDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BandejaVigilancia(CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerBandejaVigilanciaQuery(), cancellationToken));


        /// <summary>Adjunta el anexo de salida (PDF/JPG/PNG, máx 10 MB) a la propia solicitud.</summary>
        [HttpPost("{id:int}/anexo-salida")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubirAnexoSalida(int id, IFormFile archivo, CancellationToken cancellationToken)
        {
            if (archivo is null || archivo.Length == 0)
                return BadRequest("Debes adjuntar un archivo.");

            await using var stream = archivo.OpenReadStream();
            await dispatcher.Send(
                new SubirAnexoSalidaCommand(id, stream, archivo.FileName, archivo.ContentType, archivo.Length),
                cancellationToken);
            return NoContent();
        }


        /// <summary>Adjunta el anexo de retorno; al hacerlo, la solicitud queda completada.</summary>
        [HttpPost("{id:int}/anexo-retorno")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubirAnexoRetorno(int id, IFormFile archivo, CancellationToken cancellationToken)
        {
            if (archivo is null || archivo.Length == 0)
                return BadRequest("Debes adjuntar un archivo.");

            await using var stream = archivo.OpenReadStream();
            await dispatcher.Send(
                new SubirAnexoRetornoCommand(id, stream, archivo.FileName, archivo.ContentType, archivo.Length),
                cancellationToken);
            return NoContent();
        }


        /// <summary>Lista los anexos de una solicitud.</summary>
        [HttpGet("{id:int}/anexos")]
        [ProducesResponseType<IReadOnlyList<AnexoDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ListarAnexos(int id, CancellationToken cancellationToken)
            => Ok(await dispatcher.Query(new ObtenerAnexosQuery(id), cancellationToken));


        /// <summary>Descarga un anexo por su identificador.</summary>
        [HttpGet("anexos/{anexoId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DescargarAnexo(int anexoId, CancellationToken cancellationToken)
        {
            var archivo = await dispatcher.Query(new DescargarAnexoQuery(anexoId), cancellationToken);
            return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
        }
    }
}
